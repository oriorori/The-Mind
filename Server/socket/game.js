module.exports = function(server) {
    const io = require('socket.io')(server);
    const {setupRoomHandlers} = require('./roomHandler');
    const setupInGameHandlers = require('./inGameHandler');

    io.on('connection', (socket) => {
        console.log(`사용자 접속: ${socket.id}`);

        setupRoomHandlers(io, socket);
        setupInGameHandlers(io, socket);
   });
}