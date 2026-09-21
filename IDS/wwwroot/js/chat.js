/* Employee chat preview. Keep plaintext and private keys inside this browser. */
(() => {
    "use strict";

    const root = document.querySelector(".chat-page");
    if (!root) return;

    const userId = root.dataset.currentUser;
    const elements = {
        status: document.getElementById("chatConnectionStatus"),
        error: document.getElementById("chatError"),
        employees: document.getElementById("chatEmployees"),
        department: document.getElementById("departmentFilter"),
        unreadCount: document.getElementById("chatUnreadCount"),
        ownFingerprint: document.getElementById("ownFingerprint"),
        peerName: document.getElementById("chatPeerName"),
        peerDepartment: document.getElementById("chatPeerDepartment"),
        peerFingerprint: document.getElementById("chatPeerFingerprint"),
        verify: document.getElementById("chatVerify"),
        confirm: document.getElementById("chatConfirmFingerprint"),
        messages: document.getElementById("chatMessages"),
        compose: document.getElementById("chatCompose"),
        text: document.getElementById("chatText"),
        send: document.getElementById("chatSend"),
        notifications: document.getElementById("chatEnableNotifications")
    };

    const encoder = new TextEncoder();
    const decoder = new TextDecoder();
    const unread = new Set();
    const originalTitle = document.title;
    let employees = [];
    let selected = null;
    let localKeys = null;
    let ownKey = null;
    let connection = null;

    function showError(message) {
        elements.error.textContent = message;
        elements.error.hidden = false;
    }

    function clearError() {
        elements.error.hidden = true;
        elements.error.textContent = "";
    }

    function encodeBase64(bytes) {
        return btoa(String.fromCharCode(...new Uint8Array(bytes)));
    }

    function decodeBase64(value) {
        return Uint8Array.from(atob(value), character => character.charCodeAt(0));
    }

    function fingerprint(bytes) {
        return crypto.subtle.digest("SHA-256", bytes).then(hash =>
            Array.from(new Uint8Array(hash), byte => byte.toString(16).padStart(2, "0"))
                .join("").toUpperCase());
    }

    function openKeyDatabase() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open("ids-chat-keys-v1", 1);
            request.onupgradeneeded = () => request.result.createObjectStore("keys");
            request.onsuccess = () => resolve(request.result);
            request.onerror = () => reject(request.error);
        });
    }

    async function storedKeys() {
        const database = await openKeyDatabase();
        return new Promise((resolve, reject) => {
            const transaction = database.transaction("keys", "readonly");
            const request = transaction.objectStore("keys").get(userId);
            request.onsuccess = () => resolve(request.result || null);
            request.onerror = () => reject(request.error);
            transaction.oncomplete = () => database.close();
        });
    }

    async function saveKeys(keys) {
        const database = await openKeyDatabase();
        return new Promise((resolve, reject) => {
            const transaction = database.transaction("keys", "readwrite");
            transaction.objectStore("keys").put(keys, userId);
            transaction.oncomplete = () => { database.close(); resolve(); };
            transaction.onerror = () => reject(transaction.error);
        });
    }

    async function prepareKeys() {
        const registered = await connection.invoke("GetMyKey");
        localKeys = await storedKeys();
        if (!localKeys && registered) {
            throw new Error("This account's chat key belongs to another browser. Message history cannot be opened here. Contact an administrator for a reviewed key recovery process.");
        }
        if (!localKeys) {
            // The private key is nonextractable and saved only in IndexedDB on this device.
            localKeys = await crypto.subtle.generateKey(
                { name: "ECDH", namedCurve: "P-256" }, false, ["deriveBits"]);
            await saveKeys(localKeys);
        }

        const publicBytes = await crypto.subtle.exportKey("spki", localKeys.publicKey);
        const localFingerprint = await fingerprint(publicBytes);
        if (registered && registered.fingerprint !== localFingerprint) {
            throw new Error("This browser's chat key does not match the key registered for your account. Messaging is locked.");
        }
        ownKey = registered || await connection.invoke("RegisterKey", encodeBase64(publicBytes));
        elements.ownFingerprint.textContent = ownKey.fingerprint;
    }

    // This small client speaks SignalR's version 1 JSON protocol over the browser's native WebSocket.
    // It avoids loading third-party JavaScript alongside the browser-held encryption key.
    class ChatConnection {
        constructor() {
            this.socket = null;
            this.pingTimer = null;
            this.nextId = 1;
            this.pending = new Map();
            this.listeners = new Map();
            this.buffer = "";
        }

        on(event, callback) { this.listeners.set(event, callback); }

        connect() {
            return new Promise((resolve, reject) => {
                const protocol = location.protocol === "https:" ? "wss:" : "ws:";
                const socket = new WebSocket(`${protocol}//${location.host}/hubs/chat`);
                this.socket = socket;
                let ready = false;
                socket.onopen = () => socket.send('{"protocol":"json","version":1}\x1e');
                socket.onmessage = event => {
                    this.buffer += event.data;
                    const frames = this.buffer.split("\x1e");
                    this.buffer = frames.pop();
                    for (const frame of frames) {
                        if (!frame) continue;
                        let message;
                        try { message = JSON.parse(frame); }
                        catch { continue; }
                        if (!ready) {
                            if (message.error) { reject(new Error(message.error)); socket.close(); return; }
                            ready = true;
                            this.pingTimer = setInterval(() => {
                                if (socket.readyState === WebSocket.OPEN) socket.send('{"type":6}\x1e');
                            }, 12000);
                            resolve();
                            continue;
                        }
                        this.handle(message);
                    }
                };
                socket.onerror = () => { if (!ready) reject(new Error("Could not connect to chat.")); };
                socket.onclose = () => {
                    clearInterval(this.pingTimer);
                    if (!ready) reject(new Error("Chat connection closed."));
                    for (const pending of this.pending.values()) pending.reject(new Error("Chat connection closed."));
                    this.pending.clear();
                    this.listeners.get("Disconnected")?.();
                };
            });
        }

        handle(message) {
            if (message.type === 1) this.listeners.get(message.target)?.(...(message.arguments || []));
            if (message.type === 3 && this.pending.has(message.invocationId)) {
                const pending = this.pending.get(message.invocationId);
                this.pending.delete(message.invocationId);
                if (message.error) pending.reject(new Error(message.error));
                else pending.resolve(message.result);
            }
            if (message.type === 7) this.socket?.close();
        }

        invoke(target, ...args) {
            if (this.socket?.readyState !== WebSocket.OPEN)
                return Promise.reject(new Error("Chat is disconnected."));
            const invocationId = String(this.nextId++);
            return new Promise((resolve, reject) => {
                this.pending.set(invocationId, { resolve, reject });
                this.socket.send(JSON.stringify({ type: 1, invocationId, target, arguments: args }) + "\x1e");
            });
        }
    }

    function pinKey() { return `ids-chat-pinned-v1:${userId}:${selected?.id}`; }
    function isVerified() { return !!selected?.key && localStorage.getItem(pinKey()) === selected.key.fingerprint; }

    function updateUnread() {
        elements.unreadCount.hidden = unread.size === 0;
        elements.unreadCount.textContent = String(unread.size);
        document.title = unread.size ? `(${unread.size}) ${originalTitle}` : originalTitle;
        renderEmployees();
    }

    function renderEmployees() {
        const filter = elements.department.value;
        elements.employees.replaceChildren();
        const visible = employees.filter(employee => !filter || employee.department === filter);
        if (!visible.length) {
            const empty = document.createElement("p");
            empty.className = "chat-placeholder p-3";
            empty.textContent = "No employees in this department.";
            elements.employees.append(empty);
            return;
        }
        for (const employee of visible) {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "chat-employee" + (employee.id === selected?.id ? " active" : "");
            button.setAttribute("role", "option");
            button.setAttribute("aria-selected", String(employee.id === selected?.id));
            const avatar = document.createElement("span");
            avatar.className = "chat-avatar";
            avatar.textContent = employee.name.trim().charAt(0).toUpperCase() || "?";
            const details = document.createElement("span");
            details.className = "chat-employee-details";
            const name = document.createElement("strong");
            name.textContent = employee.name;
            const department = document.createElement("small");
            department.textContent = employee.department || "General";
            details.append(name, department);
            button.append(avatar, details);
            if (unread.has(employee.id)) {
                const dot = document.createElement("span");
                dot.className = "chat-employee-unread";
                dot.setAttribute("aria-label", "Unread message");
                button.append(dot);
            }
            button.addEventListener("click", () => selectEmployee(employee));
            elements.employees.append(button);
        }
    }

    function populateDepartments() {
        const departments = [...new Set(employees.map(employee => employee.department || "General"))].sort();
        elements.department.replaceChildren(new Option("All departments", ""));
        departments.forEach(department => elements.department.add(new Option(department, department)));
    }

    function refreshCompose() {
        const ready = !!selected?.key && isVerified() && !!ownKey && connection?.socket?.readyState === WebSocket.OPEN;
        elements.text.disabled = !ready;
        elements.send.disabled = !ready;
        elements.verify.hidden = !selected?.key || isVerified();
        if (selected?.key) elements.peerFingerprint.textContent = selected.key.fingerprint;
    }

    async function selectEmployee(employee) {
        selected = employee;
        clearError();
        elements.peerName.textContent = employee.name;
        elements.peerDepartment.textContent = employee.department || "General";
        elements.messages.replaceChildren();
        if (!employee.key) {
            addPlaceholder("This colleague has not opened chat on their device yet.");
        } else if (!isVerified()) {
            addPlaceholder("Verify this colleague's key fingerprint to open the conversation.");
        } else {
            await loadConversation();
        }
        refreshCompose();
        renderEmployees();
    }

    function addPlaceholder(text) {
        const element = document.createElement("p");
        element.className = "chat-placeholder";
        element.textContent = text;
        elements.messages.append(element);
    }

    function messageContext(message, copy) {
        return `${message.clientMessageId}|${message.senderId}|${message.recipientId}|${message.senderKeyFingerprint}|${message.recipientKeyFingerprint}|${copy}`;
    }

    async function messageKey(publicKeyInfo, message, copy) {
        const publicKey = await crypto.subtle.importKey("spki", decodeBase64(publicKeyInfo.subjectPublicKeyInfo),
            { name: "ECDH", namedCurve: "P-256" }, false, []);
        const sharedSecret = await crypto.subtle.deriveBits(
            { name: "ECDH", public: publicKey }, localKeys.privateKey, 256);
        const keyMaterial = await crypto.subtle.importKey("raw", sharedSecret, "HKDF", false, ["deriveKey"]);
        const salt = await crypto.subtle.digest("SHA-256", encoder.encode(`IDS chat v1 salt|${message.clientMessageId}`));
        return crypto.subtle.deriveKey({ name: "HKDF", hash: "SHA-256", salt,
            info: encoder.encode(`IDS chat v1 key|${messageContext(message, copy)}`) },
        keyMaterial, { name: "AES-GCM", length: 256 }, false, ["encrypt", "decrypt"]);
    }

    async function encryptCopy(plaintext, publicKeyInfo, message, copy) {
        const key = await messageKey(publicKeyInfo, message, copy);
        const nonce = crypto.getRandomValues(new Uint8Array(12));
        const ciphertext = await crypto.subtle.encrypt({ name: "AES-GCM", iv: nonce,
            additionalData: encoder.encode(messageContext(message, copy)) }, key, encoder.encode(plaintext));
        return { ciphertext: encodeBase64(ciphertext), nonce: encodeBase64(nonce) };
    }

    async function decryptMessage(message) {
        const mine = message.senderId === userId;
        const copy = mine ? "sender" : "recipient";
        if (mine && message.senderKeyFingerprint !== ownKey.fingerprint) throw new Error("Sender key mismatch");
        if (!mine && (message.recipientKeyFingerprint !== ownKey.fingerprint ||
            message.senderKeyFingerprint !== selected.key.fingerprint)) throw new Error("Peer key mismatch");
        const key = await messageKey(mine ? ownKey : selected.key, message, copy);
        const ciphertext = decodeBase64(mine ? message.senderCiphertext : message.recipientCiphertext);
        const nonce = decodeBase64(mine ? message.senderNonce : message.recipientNonce);
        const plaintext = await crypto.subtle.decrypt({ name: "AES-GCM", iv: nonce,
            additionalData: encoder.encode(messageContext(message, copy)) }, key, ciphertext);
        return decoder.decode(plaintext);
    }

    async function loadConversation() {
        if (!selected || !isVerified()) return;
        const peerId = selected.id;
        try {
            const messages = await connection.invoke("GetConversation", peerId, null);
            if (selected?.id !== peerId) return;
            elements.messages.replaceChildren();
            if (!messages.length) addPlaceholder("No messages yet. Say hello to start the conversation.");
            for (const message of messages) {
                let plaintext;
                try { plaintext = await decryptMessage(message); }
                catch { plaintext = "Unable to decrypt this message on this device."; }
                const bubble = document.createElement("article");
                bubble.className = "chat-bubble " + (message.senderId === userId ? "mine" : "theirs");
                bubble.dataset.messageId = message.id;
                const content = document.createElement("p");
                content.textContent = plaintext;
                const meta = document.createElement("span");
                meta.className = "chat-bubble-meta";
                const time = new Date(message.sentAtUtc).toLocaleTimeString([], { hour: "numeric", minute: "2-digit" });
                meta.textContent = message.senderId === userId
                    ? `${time} ${message.readAtUtc ? "✓✓ Read" : "✓ Sent"}` : time;
                bubble.append(content, meta);
                elements.messages.append(bubble);
                if (message.recipientId === userId && !message.readAtUtc && !document.hidden)
                    connection.invoke("MarkRead", message.id).catch(error => showError(error.message));
            }
            elements.messages.scrollTop = elements.messages.scrollHeight;
            if (!document.hidden) { unread.delete(peerId); updateUnread(); }
        } catch (error) { showError(error.message); }
    }

    async function sendMessage(event) {
        event.preventDefault();
        if (!selected?.key || !isVerified()) return;
        const plaintext = elements.text.value.trim();
        if (!plaintext || plaintext.length > 2000) return;
        elements.send.disabled = true;
        clearError();
        const message = {
            senderId: userId, recipientId: selected.id,
            clientMessageId: crypto.randomUUID(),
            senderKeyFingerprint: ownKey.fingerprint,
            recipientKeyFingerprint: selected.key.fingerprint
        };
        try {
            const sender = await encryptCopy(plaintext, ownKey, message, "sender");
            const recipient = await encryptCopy(plaintext, selected.key, message, "recipient");
            await connection.invoke("SendMessage", {
                recipientId: message.recipientId,
                clientMessageId: message.clientMessageId,
                senderCiphertext: sender.ciphertext,
                recipientCiphertext: recipient.ciphertext,
                senderNonce: sender.nonce,
                recipientNonce: recipient.nonce,
                senderKeyFingerprint: message.senderKeyFingerprint,
                recipientKeyFingerprint: message.recipientKeyFingerprint
            });
            elements.text.value = "";
            await loadConversation();
        } catch (error) { showError(error.message); }
        finally { refreshCompose(); elements.text.focus(); }
    }

    async function start() {
        if (!crypto?.subtle || !indexedDB || !window.WebSocket) {
            showError("This browser does not support the required chat security features.");
            return;
        }
        connection = new ChatConnection();
        connection.on("MessageReceived", async message => {
            unread.add(message.senderId);
            updateUnread();
            const sender = employees.find(employee => employee.id === message.senderId);
            if (document.hidden && "Notification" in window && Notification.permission === "granted") {
                new Notification("New employee message", { body: `${sender?.name || "A colleague"} sent a message.` });
            }
            if (selected?.id === message.senderId && !document.hidden) await loadConversation();
        });
        connection.on("MessageRead", (messageId, readAtUtc) => {
            const bubble = [...elements.messages.querySelectorAll(".chat-bubble.mine")]
                .find(item => item.dataset.messageId === String(messageId));
            if (bubble) {
                const meta = bubble.querySelector(".chat-bubble-meta");
                meta.textContent = meta.textContent.replace(/✓ Sent$|✓✓ Read$/, "✓✓ Read");
            }
        });
        connection.on("Disconnected", () => {
            elements.status.textContent = "Disconnected";
            refreshCompose();
            setTimeout(reconnect, 3000);
        });
        try {
            await connection.connect();
            elements.status.textContent = "Connected";
            await prepareKeys();
            employees = await connection.invoke("GetEmployees", null);
            populateDepartments();
            renderEmployees();
        } catch (error) {
            elements.status.textContent = "Unavailable";
            showError(error.message);
        }
    }

    async function reconnect() {
        try {
            connection = new ChatConnection();
            connection.on("MessageReceived", message => {
                unread.add(message.senderId); updateUnread();
                if (selected?.id === message.senderId && !document.hidden) loadConversation();
            });
            connection.on("MessageRead", () => { if (selected) loadConversation(); });
            connection.on("Disconnected", () => {
                elements.status.textContent = "Disconnected";
                refreshCompose();
                setTimeout(reconnect, 5000);
            });
            await connection.connect();
            elements.status.textContent = "Connected";
            if (!ownKey) {
                await prepareKeys();
                employees = await connection.invoke("GetEmployees", null);
                populateDepartments();
                renderEmployees();
            }
            refreshCompose();
            if (selected && isVerified()) await loadConversation();
        } catch { setTimeout(reconnect, 5000); }
    }

    elements.department.addEventListener("change", renderEmployees);
    elements.confirm.addEventListener("click", async () => {
        if (!selected?.key) return;
        localStorage.setItem(pinKey(), selected.key.fingerprint);
        refreshCompose();
        await loadConversation();
    });
    elements.compose.addEventListener("submit", sendMessage);
    elements.notifications.addEventListener("click", async () => {
        if (!("Notification" in window)) { showError("Browser notifications are unavailable here."); return; }
        const permission = await Notification.requestPermission();
        elements.notifications.textContent = permission === "granted" ? "Alerts enabled" : "Enable alerts";
    });
    document.addEventListener("visibilitychange", () => {
        if (!document.hidden && selected && isVerified()) loadConversation();
    });

    start();
})();
