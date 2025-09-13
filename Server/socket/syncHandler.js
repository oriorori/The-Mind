
const WINDOW_MS = 180;
const MAX_REWIND_MS = 250;

function syncClients(io, socket){
    // 플레이어(소켓)별로 가지고 있어야하는 정보이므로 안에서 정의

    // ping-pong (서버는 즉시 응답만)
    socket.on('pingSync', ( clientTime, seq ) => {
        socket.emit('pongSyncCli', { serverTime: Date.now(), seq: seq });
    });

    // 클라가 계산한 결과를 서버에 알려주면, 서버는 EMA로 완만히 반영
    socket.on('syncResult', ( estimatedOffset, rtt ) => {
        if(Math.abs(estimatedOffset) > 10000 || rtt > 10000) return; // 이상치 거르기
        socket.data.offset = socket.data.offset === 0 ? estimatedOffset : (socket.data.offset * 0.9 + estimatedOffset * 0.1);
        socket.data.rttEMA = socket.data.rttEMA == null ? rtt : (socket.data.rttEMA * 0.8 + rtt * 0.2);
    });
}

function ensureRoomRuntime(room) {
    if (!room.pendingPlays) room.pendingPlays = []; // {playerId, card, eventTime}
    if (!room.windowTimer) room.windowTimer = null;
    if (!room.inWindow) room.inWindow = false;
    if (!room.windowPlayedSet) room.windowPlayedSet = new Set();
}

function computeEventTime(now, clientSentTime, offset){
    let t = clientSentTime + (offset || 0);
    const lower = now - MAX_REWIND_MS; // 너무 과거 요청 clamp
    if (t < lower) t = lower;
    if (t > now)   t = now;            // 미래 타임스탬프 방지
    return t;
}

module.exports = {
    syncClients,
    ensureRoomRuntime,
    computeEventTime,
    WINDOW_MS
};