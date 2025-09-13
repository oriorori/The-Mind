module.exports = function(server) {
    const io = require('socket.io')(server);
    const {setupRoomHandlers, removePlayerFromRoom} = require('./roomHandler');
    const setupInGameHandlers = require('./inGameHandler');
    const {syncClients} = require('./syncHandler');

    io.on('connection', (socket) => {
        console.log(`사용자 접속: ${socket.id}`);

        socket.data.offset = 0;
        socket.data.rttEMA = null;

        socket.on('disconnect', (reason) => {
            console.log(`${socket.id} disconnected: ${reason}`);

            // 여기서 방에서 해당 유저를 제거하면 돼요
            const roomId = socket.data.roomId;
            const playerId = socket.data.playerId;
            if (roomId) {
                removePlayerFromRoom(io, roomId, playerId);
            }
        });

        setupRoomHandlers(io, socket);
        syncClients(io, socket);
        setupInGameHandlers(io, socket);
   });
}