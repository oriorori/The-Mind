const shurikenVotes = {};
const stageData = require('../config/stageConfig');
const roomHandler = require('./roomHandler');
const roomInfos = roomHandler.roomInfos; // 방 정보 가져오기

module.exports = (io, socket) => {

    socket.on('startStage', async () => { // 각 스테이지 시작 로직
        try{
            const roomId = socket.data.roomId;
            if(roomInfos[roomId].playing) {
                console.log(`스테이지가 이미 진행 중입니다.`);
                return;
            }
            // 다른 클라이언트에서 같은 로직을 실행하지 못하도록 함
            roomInfos[roomId].playing = true; // 스테이지 시작 시 playing 상태를 true로 설정

            const roomSize = roomInfos[socket.data.roomId].roomSize;
            const cardCount = stageData.cardPerPlayer[roomInfos[roomId].currentStage];

            const totalCardCount = cardCount * roomSize;
            // 1. 1~100 배열 만들기
            const numberPool = Array.from({ length: 100 }, (_, i) => i + 1);

            // 2. 셔플 (Fisher-Yates 알고리즘)
            for (let i = numberPool.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [numberPool[i], numberPool[j]] = [numberPool[j], numberPool[i]];
            }

            // 3. 플레이어별 카드 분배
            const players = roomInfos[roomId].players;
            const socketsInRoom = await io.in(roomId).fetchSockets();

            // 4. 분배 및 개인 전송
            for (let i = 0; i < roomSize; i++) {
                const playerId = players[i];
                const targetSocket = socketsInRoom.find(s => s.data.playerId === playerId);

                const cards = numberPool.slice(i * cardCount, (i + 1) * cardCount).sort((a, b) => a - b);

                if (targetSocket) {
                    console.log(`플레이어 ${playerId}에게 카드 ${cards}를 전송합니다.`);
                    targetSocket.emit('receiveCardsCli', cards);
                }
            }
        }
        catch (error) {
            console.error('스테이지 시작 에러:', error);
            socket.emit('errorCli', '스테이지 시작 중 오류가 발생했습니다.');
        }
    });

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
}