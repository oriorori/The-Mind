const stageData = require('../config/stageConfig');

// 공통된 룸 안에 있는 소켓들이 공유해야하는 정보이므로 바깥에서 정의
const roomInfos = {};
const gameStartVotes = {};

function setupRoomHandlers(io, socket){
    // 플레이어(소켓)별로 가지고 있어야하는 정보이므로 안에서 정의
    const playerId = '';
    const roomId = 0;

    socket.on('joinGame', ({playerId, roomId, roomSize}) => { // 방 입장 로직
    
        console.log(`사용자 ${playerId}님이 방 #${roomId}에 입장했어요`);
        socket.join(roomId); // 소켓을 roomId에 연결

        if(!roomInfos[roomId]) {
            roomInfos[roomId] = {
                players: [],
                roomSize: roomSize,
                waiting: false,
                currentStage: 0,
                remainingLife: 0,
                remainingShurikens: 0,
                playing: false
            };
            gameStartVotes[roomId] = new Set();
        }
        roomInfos[roomId].players.push(playerId);

        console.log(`현재 방 #${roomId}의 플레이어: ${roomInfos[roomId].players}`);
        
    // socket.to.emit("이벤트 이름", 데이터1, 데이터2, ...);
    // socket.to(roomId) -> roomId에 연결되어 있는 모든 소켓에게 이벤트를 보냄(자신은 제외)
    // io.to(roomId) -> roomId에 연결되어 있는 모든 소켓에게 이벤트(자신 포함)
        socket.to(roomId).emit('joinRoomCli', playerId);

        socket.data.playerId = playerId; // socket에 유저 데이터 저장
        socket.data.roomId = roomId; // socket에 방 데이터 저장
    });

    socket.on('suggestStartGame', () => { // 누군가 startgame 버튼 눌렀을 때 로직

        console.log(`사용자 ${socket.data.playerId}님이 방 #${socket.data.roomId}에서 게임 시작을 제안했어요`);
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;

        if(!gameStartVotes[roomId]){
            gameStartVotes[roomId] = new Set();
            gameStartVotes[roomId].add(playerId);
        }
        else if(!gameStartVotes[roomId].has(playerId)){
            gameStartVotes[roomId].add(playerId);
        }

        io.to(roomId).emit('suggestStartGameCli', playerId);
    });

    // when someone agree with game start
    socket.on('readyGame', () => { // 준비완료 버튼 눌렀을 때 로직
        console.log(`사용자 ${socket.data.playerId}님이 방 #${socket.data.roomId}에서 게임 시작에 동의했어요`);
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;

        // if(!gameStartVotes[roomId]) gameStartVotes[roomId] = new Set();
        if(gameStartVotes[roomId].has(playerId)){
            return;} // 이미 투표한 유저는 다시 투표하지 않도록

        gameStartVotes[roomId].add(playerId);
        
        const voteCount = gameStartVotes[roomId].size;
        const roomSize = roomInfos[roomId].roomSize;

        console.log(`현재 방 #${roomId}의 게임 시작 투표: ${voteCount}/${roomSize}`);

        if(voteCount >= roomSize){
            console.log(`방 #${roomId}에서 게임 시작!`);

            roomInfos[roomId].currentStage = 1;
            roomInfos[roomId].remainingLife = stageData.startingLife[roomSize];
            roomInfos[roomId].remainingShurikens = 1;

            io.to(roomId).emit('startGameCli', {
                roomSize: roomSize,
                currentStage: roomInfos[roomId].currentStage,
                remainingLife: roomInfos[roomId].remainingLife,
                remainingShurikens: roomInfos[roomId].remainingShurikens
            });
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

    socket.on('leaveRoom', () => {
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;
        socket.leave(roomId);
        socket.to(roomId).emit('leaveRoomCli', playerId);

        socket.data.roomId = '';
        socket.data.playerId = '';
    });
}

module.exports = {
    setupRoomHandlers,
    roomInfos,
    gameStartVotes
};

// module.exports = function(io, socket) {
//     // 플레이어(소켓)별로 가지고 있어야하는 정보이므로 안에서 정의
//     const playerId = '';
//     const roomId = 0;

//     socket.on('joinGame', ({playerId, roomId, roomSize}) => { // 방 입장 로직
    
//         console.log(`사용자 ${playerId}님이 방 #${roomId}에 입장했어요`);
//         socket.join(roomId); // 소켓을 roomId에 연결

//         if(!roomInfos[roomId]) {
//             roomInfos[roomId] = {
//                 players: [],
//                 roomSize: roomSize,
//                 waiting: false,
//                 stageNum: 0,
//                 remainingLife: 0,
//                 remainingShurikens: 0
//             };
//             gameStartVotes[roomId] = new Set();
//         }
//         roomInfos[roomId].players.push(playerId);

//         console.log(`현재 방 #${roomId}의 플레이어: ${roomInfos[roomId].players}`);
        
//     // socket.to.emit("이벤트 이름", 데이터1, 데이터2, ...);
//     // socket.to(roomId) -> roomId에 연결되어 있는 모든 소켓에게 이벤트를 보냄(자신은 제외)
//     // io.to(roomId) -> roomId에 연결되어 있는 모든 소켓에게 이벤트(자신 포함)
//         socket.to(roomId).emit('joinRoomCli', playerId);

//         socket.data.playerId = playerId; // socket에 유저 데이터 저장
//         socket.data.roomId = roomId; // socket에 방 데이터 저장
//     });

//     socket.on('suggestStartGame', () => { // 누군가 startgame 버튼 눌렀을 때 로직

//         console.log(`사용자 ${socket.data.playerId}님이 방 #${socket.data.roomId}에서 게임 시작을 제안했어요`);
//         const roomId = socket.data.roomId;
//         const playerId = socket.data.playerId;

//         if(!gameStartVotes[roomId]){
//             gameStartVotes[roomId] = new Set();
//             gameStartVotes[roomId].add(playerId);
//         }
//         else if(!gameStartVotes[roomId].has(playerId)){
//             gameStartVotes[roomId].add(playerId);
//         }

//         io.to(roomId).emit('suggestStartGameCli', playerId);
//     });

//     // when someone agree with game start
//     socket.on('readyGame', () => { // 준비완료 버튼 눌렀을 때 로직
//         console.log(`사용자 ${socket.data.playerId}님이 방 #${socket.data.roomId}에서 게임 시작에 동의했어요`);
//         const roomId = socket.data.roomId;
//         const playerId = socket.data.playerId;

//         // if(!gameStartVotes[roomId]) gameStartVotes[roomId] = new Set();
//         if(gameStartVotes[roomId].has(playerId)){
//             return;} // 이미 투표한 유저는 다시 투표하지 않도록

//         gameStartVotes[roomId].add(playerId);
        
//         const voteCount = gameStartVotes[roomId].size;
//         const roomSize = roomInfos[roomId].roomSize;

//         console.log(`현재 방 #${roomId}의 게임 시작 투표: ${voteCount}/${roomSize}`);

//         if(voteCount >= roomSize){
//             io.to(roomId).emit('startGameCli');
//             gameStartVotes[roomId].clear(); // 투표 초기화
//         }
//         else{
//             io.to(roomId).emit('readyGameCli', playerId);
//         }
//     });

//     socket.on('refuseGame', () => {
//         const roomId = socket.data.roomId;
//         gameStartVotes[roomId].clear();
//         socket.to(roomId).emit('refuseGameCli');
//     })

//     socket.on('leaveRoom', () => {
//         const roomId = socket.data.roomId;
//         const playerId = socket.data.playerId;
//         socket.leave(roomId);
//         socket.to(roomId).emit('leaveRoomCli', playerId);

//         socket.data.roomId = '';
//         socket.data.playerId = '';
//     });
// };