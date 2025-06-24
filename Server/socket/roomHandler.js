const stageData = require('../config/stageConfig');
const { createRoomInfo } = require('../utils/inGameUtil');
const { destroyRoom } = require('../controllers/roomController');

// 공통된 룸 안에 있는 소켓들이 공유해야하는 정보이므로 바깥에서 정의
const roomInfos = {};

function setupRoomHandlers(io, socket){
    // 플레이어(소켓)별로 가지고 있어야하는 정보이므로 안에서 정의
    const playerId = '';
    const roomId = null;

    socket.on('joinGame', ({playerId, roomId, roomSize}) => { // 방 입장 로직
    
        console.log(`사용자 ${playerId}님이 방 #${roomId}에 입장했어요`);
        socket.join(roomId); // 소켓을 roomId에 연결

        if(!roomInfos[roomId]) {
            roomInfos[roomId] = createRoomInfo(roomSize); // 방 정보 초기화
        }
        roomInfos[roomId].players.push(playerId);
        roomInfos[roomId].inWaitingRoom.push(playerId);

        console.log(`현재 방 #${roomId}의 플레이어: ${roomInfos[roomId].players}`);
        
    // socket.to.emit("이벤트 이름", 데이터1, 데이터2, ...);
    // socket.to(roomId) -> roomId에 연결되어 있는 모든 소켓에게 이벤트를 보냄(자신은 제외)
    // io.to(roomId) -> roomId에 연결되어 있는 모든 소켓에게 이벤트(자신 포함)
        socket.to(roomId).emit('joinRoomCli', playerId);

        socket.data.playerId = playerId; // socket에 유저 데이터 저장
        socket.data.roomId = roomId; // socket에 방 데이터 저장
    });

    socket.on('backToRoom',()=>{
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;

        if(!roomInfos[roomId]) {
            console.error(`방 #${roomId}이 존재하지 않습니다.`);
            return;
        }

        if(!roomInfos[roomId]) {
            console.error(`방 #${roomId}이 존재하지 않습니다.`);
            socket.emit('errorCli', '방이 존재하지 않습니다.');
            return;
        }

        // 게임 상태 초기화
        roomInfos[roomId].playing = false;
        roomInfos[roomId].waiting = false;
        roomInfos[roomId].currentStage = 1;
        roomInfos[roomId].remainingLife = stageData.startingLife[roomInfos[roomId].roomSize];
        roomInfos[roomId].remainingShurikens = 1;
        roomInfos[roomId].cards = {}; // 카드 초기화
        roomInfos[roomId].shuffling = false; // 셔플 상태 초기화


        roomInfos[roomId].inWaitingRoom.push(playerId); // 대기실에 다시 추가
        io.to(roomId).emit('backToRoomCli', playerId);
        console.log(`사용자 ${playerId}님이 방 #${roomId}에 다시 입장했어요`);
    })

    socket.on('suggestStartGame', () => { // 누군가 startgame 버튼 눌렀을 때 로직

        console.log(`사용자 ${socket.data.playerId}님이 방 #${socket.data.roomId}에서 게임 시작을 제안했어요`);
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;


        if(!roomInfos[roomId]) {
            console.error(`방 #${roomId}이 존재하지 않습니다.`);
            return;
        }
        else if(!roomInfos[roomId].gameStartVotes.has(playerId)){
            roomInfos[roomId].gameStartVotes.add(playerId);
            io.to(roomId).emit('suggestStartGameCli', playerId);
        }
    });

    // when someone agree with game start
    socket.on('readyGame', () => { // 준비완료 버튼 눌렀을 때 로직
        console.log(`사용자 ${socket.data.playerId}님이 방 #${socket.data.roomId}에서 게임 시작에 동의했어요`);
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;
        if(!roomInfos[roomId]) {
            console.error(`방 #${roomId}이 존재하지 않습니다.`);
            return;
        }
        if(roomInfos[roomId].gameStartVotes.has(playerId)){
            return;} // 이미 투표한 유저는 다시 투표하지 않도록

        roomInfos[roomId].gameStartVotes.add(playerId);

        const voteCount = roomInfos[roomId].gameStartVotes.size;
        const roomSize = roomInfos[roomId].roomSize;

        console.log(`현재 방 #${roomId}의 게임 시작 투표: ${voteCount}/${roomSize}`);

        if(voteCount >= roomSize){
            console.log(`방 #${roomId}에서 게임 시작!`);
            roomInfos[roomId].waiting = false;
            roomInfos[roomId].playing = true; // 게임 시작 상태로 변경
            roomInfos[roomId].inWaitingRoom = []; // 대기실 초기화

            roomInfos[roomId].currentStage = 1;
            roomInfos[roomId].remainingLife = stageData.startingLife[roomSize];
            roomInfos[roomId].remainingShurikens = 1;

            io.to(roomId).emit('startGameCli', {
                roomSize: roomSize,
                currentStage: roomInfos[roomId].currentStage,
                remainingLife: roomInfos[roomId].remainingLife,
                remainingShurikens: roomInfos[roomId].remainingShurikens
            });
            roomInfos[roomId].gameStartVotes.clear(); // 투표 초기화
        }
        else{
            io.to(roomId).emit('readyGameCli', playerId);
        }
    });

    socket.on('refuseGame', () => {
        const roomId = socket.data.roomId;
        roomInfos[roomId].gameStartVotes.clear();
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

    socket.on('destroyRoom', async () =>{
        const roomId = socket.data.roomId;

        if(roomInfos[roomId]) {
            // 클라이언트에게 request 전송
            io.to(roomId).emit('destroyRoomCli');
            
            // 데이터 삭제
            destroyRoom(roomId); // roomController의 rooms 객체에서 방 삭제
            delete roomInfos[roomId]; // 방 정보 삭제

            // 연결 해제
            const socketsInRoom = await io.in(roomId).fetchSockets();
            socketsInRoom.forEach(socket => {
                socket.leave(roomId); // 방에 연결된 모든 소켓을 방에서 제거
                socket.data.roomId = null;
            });

            console.log(`방 #${roomId}이 파괴되었습니다.`);
        } 
        else {
            console.error(`방 #${roomId}이 존재하지 않습니다.`);
        }
    })
}

module.exports = {
    setupRoomHandlers,
    roomInfos,
};