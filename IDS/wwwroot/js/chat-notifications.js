/* A small SignalR listener for the navigation badge on pages outside Chat. */
(() => {
    "use strict";
    if (document.querySelector(".chat-page")) return;
    const chatLink = document.querySelector('a[href$="/Chat"]');
    if (!chatLink || !window.WebSocket) return;

    const badge = document.createElement("span");
    badge.className = "badge rounded-pill bg-danger ms-1";
    badge.hidden = true;
    badge.setAttribute("aria-label", "Unread messages");
    chatLink.append(badge);

    let socket;
    let pingTimer;
    let buffer = "";
    let invocationId = 0;
    const pending = new Map();

    function invoke(target) {
        const id = String(++invocationId);
        return new Promise((resolve, reject) => {
            pending.set(id, { resolve, reject });
            socket.send(JSON.stringify({ type: 1, invocationId: id, target, arguments: [] }) + "\x1e");
        });
    }

    function setCount(count) {
        badge.hidden = count <= 0;
        badge.textContent = count > 99 ? "99+" : String(count);
    }

    function showNotice() {
        document.getElementById("chatMessageNotice")?.remove();
        const notice = document.createElement("div");
        notice.id = "chatMessageNotice";
        notice.className = "alert alert-primary shadow d-flex align-items-center gap-3";
        notice.setAttribute("role", "status");
        Object.assign(notice.style, {
            position: "fixed", top: "5rem", right: "1rem", zIndex: "1080", maxWidth: "22rem"
        });
        const text = document.createElement("span");
        text.textContent = "You have a new employee message.";
        const open = document.createElement("a");
        open.href = chatLink.href;
        open.className = "btn btn-primary btn-sm";
        open.textContent = "Open chat";
        const close = document.createElement("button");
        close.type = "button";
        close.className = "btn-close";
        close.setAttribute("aria-label", "Dismiss notification");
        close.addEventListener("click", () => notice.remove());
        notice.append(text, open, close);
        document.body.append(notice);
        setTimeout(() => notice.remove(), 8000);
        if (document.hidden && "Notification" in window && Notification.permission === "granted")
            new Notification("New employee message", { body: "Open IDS Security to read it." });
    }

    function connect() {
        buffer = "";
        const protocol = location.protocol === "https:" ? "wss:" : "ws:";
        socket = new WebSocket(`${protocol}//${location.host}/hubs/chat`);
        let handshaken = false;
        socket.onopen = () => socket.send('{"protocol":"json","version":1}\x1e');
        socket.onmessage = event => {
            buffer += event.data;
            const frames = buffer.split("\x1e");
            buffer = frames.pop();
            for (const frame of frames) {
                if (!frame) continue;
                let message;
                try { message = JSON.parse(frame); } catch { continue; }
                if (!handshaken) {
                    if (message.error) { socket.close(); return; }
                    handshaken = true;
                    invoke("GetUnreadCount").then(setCount).catch(() => {});
                    pingTimer = setInterval(() => {
                        if (socket.readyState === WebSocket.OPEN) socket.send('{"type":6}\x1e');
                    }, 12000);
                    continue;
                }
                if (message.type === 1 && message.target === "MessageReceived") {
                    showNotice();
                    invoke("GetUnreadCount").then(setCount).catch(() => {});
                }
                if (message.type === 3 && pending.has(message.invocationId)) {
                    const request = pending.get(message.invocationId);
                    pending.delete(message.invocationId);
                    message.error ? request.reject(new Error(message.error)) : request.resolve(message.result);
                }
                if (message.type === 7) socket.close();
            }
        };
        socket.onclose = () => {
            clearInterval(pingTimer);
            for (const request of pending.values()) request.reject(new Error("Chat disconnected."));
            pending.clear();
            setTimeout(connect, 5000);
        };
    }

    connect();
})();
