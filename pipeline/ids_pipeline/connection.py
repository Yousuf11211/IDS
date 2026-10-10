"""Resolve IDS/Program.cs configuration and translate SqlClient options to ODBC."""
import json
import os
from pathlib import Path
import re
import xml.etree.ElementTree as ET


def setting(data, *keys):
    for key in keys:
        if not isinstance(data, dict):
            return None
        data = next((value for name, value in data.items() if name.lower() == key.lower()), None)
    return data


def webapp_connection(config):
    from dotenv import dotenv_values
    app = (config.root / config.webapp_dir).resolve()
    environment = os.environ.get("DOTNET_ENVIRONMENT") or os.environ.get("ASPNETCORE_ENVIRONMENT") or config.environment
    # Do not load arbitrary paths from an environment name.
    if not re.fullmatch(r"[A-Za-z0-9_-]+", environment):
        raise ValueError("Invalid ASP.NET environment name")
    connection = None
    for path in (app / "appsettings.json", app / f"appsettings.{environment}.json"):
        if path.exists():
            value = setting(json.loads(path.read_text(encoding="utf-8-sig")), "ConnectionStrings", "DefaultConnection")
            if value is not None:
                connection = value
    # WebApplication.CreateBuilder loads user secrets only in Development.
    project = app / "IDS.csproj"
    if environment.lower() == "development" and project.exists():
        secret_id = ET.parse(project).findtext(".//UserSecretsId")
        if secret_id:
            base = Path(os.environ["APPDATA"]) / "Microsoft/UserSecrets" if os.environ.get("APPDATA") else Path.home() / ".microsoft/usersecrets"
            path = base / secret_id / "secrets.json"
            if path.exists():
                secrets = json.loads(path.read_text(encoding="utf-8-sig"))
                value = setting(secrets, "ConnectionStrings:DefaultConnection") or setting(secrets, "ConnectionStrings", "DefaultConnection")
                if value is not None:
                    connection = value
    connection = os.environ.get("ConnectionStrings__DefaultConnection", connection)
    # Program.cs explicitly prefers CONNECTION_STRING after DotNetEnv.Load, which
    # overwrites existing variables. Read values without changing this process's env.
    env_file = app / ".env"
    file_values = dotenv_values(env_file, encoding="utf-8-sig") if env_file.exists() else {}
    connection = file_values.get("CONNECTION_STRING", os.environ.get("CONNECTION_STRING", connection))
    if not isinstance(connection, str) or not connection.strip():
        raise ValueError("No DefaultConnection found in the web application's configuration")
    return connection


def parse_connection(value):
    """Parse SqlClient quoting, including semicolons and doubled quote characters."""
    options = {}
    index = 0
    while index < len(value):
        while index < len(value) and (value[index].isspace() or value[index] == ";"):
            index += 1
        if index == len(value):
            break
        end = value.find("=", index)
        if end < 0:
            raise ValueError("Malformed web app connection string")
        key = value[index:end].strip().lower()
        if not key or ";" in key:
            raise ValueError("Malformed web app connection string")
        index = end + 1
        while index < len(value) and value[index].isspace():
            index += 1
        if index < len(value) and value[index] in "\"'":
            quote = value[index]
            index += 1
            parts = []
            while index < len(value):
                char = value[index]
                index += 1
                if char == quote:
                    if index < len(value) and value[index] == quote:
                        parts.append(quote)
                        index += 1
                    else:
                        break
                else:
                    parts.append(char)
            else:
                raise ValueError("Unclosed quote in web app connection string")
            item = "".join(parts)
            while index < len(value) and value[index].isspace():
                index += 1
            if index < len(value) and value[index] != ";":
                raise ValueError("Malformed quoted connection option")
        else:
            end = value.find(";", index)
            if end < 0:
                end = len(value)
            item, index = value[index:end].strip(), end
        options[key] = item
    return options


def to_odbc(connection, driver):
    aliases = {
        "server": "Server", "data source": "Server", "address": "Server", "addr": "Server", "network address": "Server",
        "database": "Database", "initial catalog": "Database",
        "user id": "UID", "uid": "UID", "user": "UID", "password": "PWD", "pwd": "PWD",
        "trusted_connection": "Trusted_Connection", "integrated security": "Trusted_Connection",
        "encrypt": "Encrypt", "trustservercertificate": "TrustServerCertificate",
        "multipleactiveresultsets": "MARS_Connection", "application name": "APP",
        "connect timeout": "Connection Timeout", "connection timeout": "Connection Timeout",
        "authentication": "Authentication", "multisubnetfailover": "MultiSubnetFailover",
        "applicationintent": "ApplicationIntent", "hostnameincertificate": "HostnameInCertificate",
    }
    ignored = {"pooling", "min pool size", "max pool size", "persist security info", "connectretrycount", "connectretryinterval"}
    result = {"Driver": driver, "Encrypt": "yes"}
    boolean = {"Trusted_Connection", "TrustServerCertificate", "MARS_Connection", "MultiSubnetFailover"}
    for key, value in parse_connection(connection).items():
        if key in ignored:
            continue
        if key not in aliases:
            raise ValueError(f"Unsupported SqlClient connection option: {key}")
        target = aliases[key]
        if target in boolean or target == "Encrypt":
            lower = value.lower()
            if lower in ("true", "yes", "sspi", "mandatory"):
                value = "yes"
            elif lower in ("false", "no", "optional"):
                value = "no"
            elif target == "Encrypt" and lower == "strict":
                value = "strict"
            else:
                raise ValueError(f"Invalid boolean connection option: {key}")
        result[target] = value
    if not result.get("Server") or not result.get("Database"):
        raise ValueError("Web app connection must specify a SQL Server and database")
    # Every value is ODBC-brace escaped: passwords may contain ;, quotes or }.
    return ";".join(key + "={" + value.replace("}", "}}") + "}" for key, value in result.items()) + ";"


def connection_string(config, driver):
    return to_odbc(webapp_connection(config), driver)
