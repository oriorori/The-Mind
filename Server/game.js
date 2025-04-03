module.exports = function(server) {
    const io = require('socket.io')(server);

    const roomInfos = {};
    const gameStartVotes = {};
    const shurikenVotes = {};

    io.on('connection', (socket) => {
        console.log(`사용자 접속: ${socket.id}`);

        var playerId = '';
        var roomId = 0;

        socket.on('joinGame', ({playerId, roomId, maxPlayerNumber}) => {
        
            console.log(`사용자 ${playerId}님이 방 #${roomId}에 입장했어요`);
            socket.join(roomId);

            if(!roomInfos[roomId]) {
                roomInfos[roomId] = {
                    players: [],
                    maxPlayerNumber: maxPlayerNumber,
                };
                gameStartVotes[roomId] = new Set();
                shurikenVotes[roomId] = new Set();
            }
            roomInfos[roomId].players.push(playerId);

            socket.to(roomId).emit('joinRoomCli', playerId);

            socket.data.playerId = playerId; // socket에 유저 데이터 저장
            socket.data.roomId = roomId; // socket에 방 데이터 저장
        });

        socket.on('startGame', () => {

            const roomId = socket.data.roomId;
            const playerId = socket.data.playerId;

            if(!gameStartVotes[roomId]) gameStartVotes[roomId] = new Set();
            gameStartVotes[roomId].add(playerId);

            io.to(roomId).emit('suggestStartGameCli');
        });

        // when someone agree with game start
        socket.on('readyGame', () => {
            const roomId = socket.data.roomId;
            const playerId = socket.data.playerId;

            if(!gameStartVotes[roomId]) gameStartVotes[roomId] = new Set();
            gameStartVotes[roomId].add(playerId);
            
            const voteCount = gameStartVotes[roomId].size;
            const roomSize = roomInfos[roomId].maxPlayerNumber;

            if(voteCount >= roomSize){
                io.to(roomId).emit('startGameCli');
                gameStartVotes[roomId].clear(); // 투표 초기화
            }
            else{
                io.to(roomId).emit('readyGameCli', playerId);
            }
        });

        socket.on('refuseGame', () => {
            const roomId = socket.data.roomId;
            gameStartVotes[roomId].clear();
            socket.to(roomId).emit('refuseGameCli');
        })

        // when someone play card
        socket.on('playCard', (cardNumber) => {
            const roomId = socket.data.roomId;
            const playerId = socket.data.playerId;
            socket.to(roomId).emit('playCardCli', cardNumber, playerId);
        });

        // use shuriken at first
        socket.on('suggestShuriken', () => {
            const roomId = socket.data.roomId;
            const playerId = socket.data.playerId;

            if(!shurikenVotes[roomId]) shurikenVotes[roomId] = new Set();
            shurikenVotes[roomId].add(playerId);

            socket.to(roomId).emit('suggestShurikenCli', playerId);
        });

        // agree with shuriken using
        socket.on('agreeShuriken', () => {
            const roomId = socket.data.roomId;
            const playerId = socket.data.playerId;

            if(!shurikenVotes[roomId]) shurikenVotes[roomId] = new Set();
            shurikenVotes[roomId].add(playerId);   

            const voteCount = shurikenVotes[roomId].size;
            const roomSize = roomInfos[roomId].maxPlayerNumber;

            if(voteCount >= roomSize){
                io.to(roomId).emit('useShurikenCli');
                shurikenVotes[roomId].clear(); // 투표 초기화
            }
            else{
                io.to(roomId).emit('agreeShurikenCli', playerId);
            }
        });

        // disagree with shuriken using
        socket.on('refuseShuriken', () => {
            const roomId = socket.data.roomId;
            shurikenVotes[roomId].clear(); // 투표 초기화
            socket.to(roomId).emit('refuseShurikenCli');
        });

        socket.on('leaveRoom', () => {
            const roomId = socket.data.roomId;
            const playerId = socket.data.playerId;
            socket.leave(roomId);
            socket.to(roomId).emit('leaveRoomCli', playerId);

            socket.data.roomId = '';
            socket.data.playerId = '';
        });
    });
};