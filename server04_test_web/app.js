"use strict";
function start() {
    {
        var span = document.createElement("span");
        span.textContent = "hello world";
        document.body.appendChild(span);
        var br = document.createElement("br");
        document.body.appendChild(br);
    }
    {
        var btn = document.createElement("button");
        btn.textContent = "link";
        btn.onclick = function (ev) {
            onBtnLink();
        };
        document.body.appendChild(btn);
        var br = document.createElement("br");
        document.body.appendChild(br);
    }
    {
        var btn = document.createElement("button");
        btn.textContent = "send";
        btn.onclick = function (ev) {
            onBtnSend();
        };
        document.body.appendChild(btn);
        var br = document.createElement("br");
        document.body.appendChild(br);
    }
}
function addText(text) {
    var span = document.createElement("span");
    span.textContent = text;
    document.body.appendChild(span);
    var br = document.createElement("br");
    document.body.appendChild(br);
}
var ws = null;
function onBtnLink() {
    if (ws != null) {
        ws.close();
        ws = null;
    }
    ws = new WebSocket("ws://localhost:1988/ws");
    ws.onopen = function (ev) {
        console.log("onopen");
        addText("websocket onopen");
    };
    ws.onerror = function (ev) {
        console.log("onerror" + ev);
        addText("websocket onerror");
    };
    ws.onclose = function (ev) {
        console.log("onclose");
        addText("websocket onclose");
    };
    ws.onmessage = function (ev) {
        if (typeof ev.data == "string") {
            //收到文本数据
            var result = ev.data;
            console.log("onmessage:" + result);
            addText("websocket onmessage(txt):" + result);
        }
        else if (ev.data instanceof Blob) {
            //收到2进制数据
            var blob = ev.data;
            var reader = new FileReader();
            reader.onloadend = function (ev2) {
                var u8 = new Uint8Array(reader.result);
                var text = Bytes2String(u8);
                console.log("onmessage:" + text);
                addText("websocket onmessage(bin):" + text);
            };
            reader.readAsArrayBuffer(blob);
        }
    };
}
function onBtnSend() {
    ws === null || ws === void 0 ? void 0 : ws.send("helloha");
}
function Bytes2String(_arr) {
    var UTF = '';
    for (var i = 0; i < _arr.length; i++) {
        var one = _arr[i].toString(2), v = one.match(/^1+?(?=0)/);
        if (v && one.length == 8) {
            var bytesLength = v[0].length;
            var store = _arr[i].toString(2).slice(7 - bytesLength);
            for (var st = 1; st < bytesLength; st++) {
                store += _arr[st + i].toString(2).slice(2);
            }
            UTF += String.fromCharCode(parseInt(store, 2));
            i += bytesLength - 1;
        }
        else {
            UTF += String.fromCharCode(_arr[i]);
        }
    }
    return UTF;
}
window.onload = function (ev) {
    start();
};
//# sourceMappingURL=app.js.map