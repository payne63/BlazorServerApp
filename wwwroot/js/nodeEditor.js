window.nodeEditor = window.nodeEditor || {};
window.nodeEditor.getRect = function (element) {
    if (!element) return { left: 0, top: 0 };
    const r = element.getBoundingClientRect();
    // Return viewport-based coordinates to pair with MouseEventArgs.ClientX/ClientY
    return { left: r.left, top: r.top };
};
