const stageData = require('../config/stageConfig');
const roomHandler = require('./roomHandler');
const syncHandler = require('./syncHandler');
const roomInfos = roomHandler.roomInfos; // 방 정보 가져오기

module.exports = (io, socket) => {

    socket.on('startStage', async () => { // 각 스테이지 시작 로직
        try{
            const roomId = socket.data.roomId;
            if(roomInfos[roomId].shuffling) {
                console.log(`스테이지가 이미 진행 중입니다.`);
                return;
            }
            // 다른 클라이언트에서 같은 로직을 실행하지 못하도록 함
            roomInfos[roomId].shuffling = true; // 스테이지 시작 시 playing 상태를 true로 설정

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

            // 4. 분배, roomInfo에 저장, 개인 전송
            for (let i = 0; i < roomSize; i++) {
                const playerId = players[i];
                const targetSocket = socketsInRoom.find(s => s.data.playerId === playerId);

                const cards = numberPool.slice(i * cardCount, (i + 1) * cardCount).sort((a, b) => a - b);

                roomInfos[roomId].cards[playerId] = cards; // 방 정보에 카드 저장

                if (targetSocket) {
                    console.log(`플레이어 ${playerId}에게 카드 ${cards}를 전송합니다.`);
                    targetSocket.emit('receiveCardsCli', cards);
                }
            }

            syncHandler.ensureRoomRuntime(roomInfos[roomId]);
        }
        catch (error) {
            console.error('스테이지 시작 에러:', error);
            socket.emit('errorCli', '스테이지 시작 중 오류가 발생했습니다.');
        }
    });

    socket.on('cardMove', (ratioToCenter, ratioToCenterVertical) => {
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;

        socket.to(roomId).emit('cardMoveCli', {
            playerId: playerId,
            ratioToCenter: ratioToCenter,
            ratioToCenterVertical: ratioToCenterVertical
        });
    })

    socket.on('rollbackCardMovement', () => {
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;

        socket.to(roomId).emit('rollbackCardMovementCli', playerId);
    })

    // when someone play card
    socket.on('playCard', (cardNumber, clientTime) => {
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;
        const lowerNumbers = {};
        let remainingCardCount = 0;
        let failed = false;
        const room = roomInfos[roomId];

        const now = Date.now();
        const eventTime = syncHandler.computeEventTime(now, clientTime, socket.data?.offset || 0);
        if(room.inWindow && room.windowPlayedSet.has(playerId)) return;

        if(!room.inWindow){
            room.inWindow = true;
            room.windowPlayedSet.clear();

            room.workCards = {};
            for(const p of room.players){
                room.workCards[p] = [...(room.cards[p] || [])];
            }

            // 타이머 시작, 윈도우 종료 시 일괄 처리
            room.windowTimer = setTimeout(() => {
                try{ resolveDecisionWindow(io, roomId);}
                finally{
                    room.inWindow = false;
                    room.windowTimer = null; // window가 열린 시간
                    room.pendingPlays.length = 0; // 플레이한 카드 정보(player, number, 기대시간)
                    room.workCards = null;
                    room.windowPlayedSet.clear(); // 단일 윈도우 내 중복 플레이 방지
                }
            }, syncHandler.WINDOW_MS);
        }

        room.pendingPlays.push({playerId, card: cardNumber, eventTime});
        room.windowPlayedSet.add(playerId);

        console.log(`방 #${roomId}에서 ${playerId}님이 ${cardNumber} 카드를 낸 것을 수집.`);

        // for (const player of roomInfos[roomId].players) {

        //     lowerNumbers[player] = roomInfos[roomId].cards[player].filter(num => num < cardNumber); // 실패한 숫자
        //     roomInfos[roomId].cards[player] = roomInfos[roomId].cards[player].filter(num => num > cardNumber); // 남은 숫자
        //     if(lowerNumbers[player].length > 0) {
        //         failed = true; // 다른 플레이어가 더 낮은 카드를 가지고 있다면 실패
        //     }
        //     remainingCardCount += roomInfos[roomId].cards[player].length; // 남은 카드 개수
        // }

        // // 실패시
        // if(failed){
        //     roomInfos[roomId].remainingLife -= 1; // 생명 감소
        //     if(roomInfos[roomId].remainingLife <= 0) {
        //         // 생명이 0이 되면 게임 종료
        //         roomInfos[roomId].playing = false; // 게임 상태 초기화
        //         io.to(roomId).emit('gameOverCli', '생명이 모두 소진되었습니다. 게임 오버!');
        //         return;
        //     }
        //     else{
        //         io.to(roomId).emit('playWrongCardCli', {
        //             playedCardNumber: cardNumber,
        //             playedPlayer: playerId,
        //             remainingLife: roomInfos[roomId].remainingLife,
        //             lowerNumbers: lowerNumbers
        //         })
        //     }
        // }
        // else{
        //     io.to(roomId).emit('playRightCardCli', {
        //         playedCardNumber: cardNumber,
        //         playedPlayer: playerId
        //     });
        // }
        
        // // 남은 카드가 없을 시 => 스테이지 클리어
        // checkStageClear(io, roomId, remainingCardCount);
    });

    // use shuriken at first
    socket.on('suggestShuriken', () => {
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;

        if(!roomInfos[roomId].shurikenVotes) roomInfos[roomId].shurikenVotes = new Set();
        roomInfos[roomId].shurikenVotes.add(playerId);

        io.to(roomId).emit('suggestShurikenCli', playerId);
    });

    // agree with shuriken using
    socket.on('agreeShuriken', () => {
        const roomId = socket.data.roomId;
        const playerId = socket.data.playerId;

        if(!roomInfos[roomId].shurikenVotes) roomInfos[roomId].shurikenVotes = new Set();
        roomInfos[roomId].shurikenVotes.add(playerId);

        const voteCount = roomInfos[roomId].shurikenVotes.size;
        const roomSize = roomInfos[roomId].roomSize;
        console.log(`방 #${roomId}에서 ${playerId}님이 수리검 사용에 동의했습니다. 현재 투표 수: ${voteCount}`);
        if(voteCount >= roomSize){
            const lowestNumbers = useShuriken(roomId); // 수리검 사용 로직 실행
            console.log(`방 #${roomId}에서 수리검 사용!`);
            io.to(roomId).emit('useShurikenCli', {
                remainingShurikens: roomInfos[roomId].remainingShurikens,
                lowestNumbers: lowestNumbers
            });
            roomInfos[roomId].shurikenVotes.clear(); // 투표 초기화
        }

        checkStageClear(io, roomId, Object.values(roomInfos[roomId].cards).reduce((acc, cards) => acc + cards.length, 0)); // 남은 카드 개수로 스테이지 클리어 체크
    
    });

    // disagree with shuriken using
    socket.on('refuseShuriken', () => {
        const roomId = socket.data.roomId;
        roomInfos[roomId].shurikenVotes.clear(); // 투표 초기화
        io.to(roomId).emit('refuseShurikenCli');
    });
}

function useShuriken(roomId) {
    roomInfos[roomId].remainingShurikens -= 1; // 수리검 사용

    const lowestNumbers = {};
    for(const player of roomInfos[roomId].players) {
        if(!roomInfos[roomId].cards[player] || roomInfos[roomId].cards[player].length === 0) {
            console.error(`플레이어 ${player}의 카드가 없습니다.`);
            lowestNumbers[player] = 0; // 카드가 없는 경우 0으로 설정
            continue;
        }

        lowestNumbers[player] = roomInfos[roomId].cards[player].shift(); // 각 플레이어의 가장 낮은 카드
    }
    return lowestNumbers; // 각 플레이어의 가장 낮은 카드 반환
}

function checkStageClear(io, roomId, remainingCardCount){
        if(remainingCardCount === 0) {
        if(roomInfos[roomId].currentStage === stageData.totalStages[roomInfos[roomId].roomSize]) {
            // 모든 스테이지를 클리어한 경우
            roomInfos[roomId].playing = false; // 게임 상태 초기화
            io.to(roomId).emit('gameClearCli', '모든 스테이지를 클리어했습니다! 축하합니다!');
            return;
        }

        roomInfos[roomId].remainingLife += stageData.getLife[roomInfos[roomId].currentStage]; // 스테이지 클리어 시 생명 회복
        roomInfos[roomId].remainingShurikens += stageData.getShuriken[roomInfos[roomId].currentStage]; // 스테이지 클리어 시 수리검 회복
        roomInfos[roomId].currentStage += 1; // 다음 스테이지로 넘어감
        roomInfos[roomId].shuffling = false; // 스테이지가 끝났으므로 셔플 상태 초기화

        io.to(roomId).emit('stageClearCli', {
            roomSize: roomInfos[roomId].roomSize,
            currentStage: roomInfos[roomId].currentStage,
            remainingLife: roomInfos[roomId].remainingLife,
            remainingShurikens: roomInfos[roomId].remainingShurikens
        });
        return;
    }
}

function resolveDecisionWindow(io, roomId){
    const room = roomInfos[roomId];
    if (!room || room.pendingPlays.length === 0) return; 

    if(room.pendingPlays.length > 1) { console.log('한번에 2개 이상의 카드가 들어왔습니다');
        console.log(`${room.pendingPlays[0].playerId}: ${room.pendingPlays[0].card} at ${room.pendingPlays[0].eventTime}`);
        console.log(`${room.pendingPlays[1].playerId}: ${room.pendingPlays[1].card} at ${room.pendingPlays[1].eventTime}`);
    }

    // 의도 시각 -> 카드 값 -> 플레이어ID 순 정렬
    room.pendingPlays.sort((a, b) =>
    (a.eventTime - b.eventTime) ||
    (a.card - b.card) ||
    (a.playerId.localeCompare(b.playerId))
    );

    let remainingCardCount = Object.values(room.workCards).reduce((acc, arr) => acc + arr.length, 0);

    for (const ev of room.pendingPlays) {
        const { playerId, card } = ev;

        // 여전히 가진 카드인지 확인(같은 윈도우 내 선처리로 인해 사라졌을 수 있음)
        const idx = room.workCards[playerId]?.indexOf(card);
        if (idx === -1) continue; // 이미 소비됨(무시)

        // 실패/성공 판정(작업용 상태 기준)
        const lowerNumbers = {};
        let failed = false;
        for (const p of room.players) {
            const lower = room.workCards[p].filter(n => n < card);
            lowerNumbers[p] = lower;
            if (p !== playerId && lower.length > 0) failed = true;
        }

        // 카드 제거(적용)
        room.workCards[playerId].splice(idx, 1);
        remainingCardCount--;

        if (failed) {
            room.remainingLife -= 1;
            io.to(roomId).emit('playWrongCardCli', {
                playedCardNumber: card,
                playedPlayer: playerId,
                remainingLife: room.remainingLife,
                lowerNumbers
            });
            if (room.remainingLife <= 0) {
                room.playing = false;
                io.to(roomId).emit('gameOverCli', '생명이 모두 소진되었습니다. 게임 오버!');
                return;
            }
        } 
        else {
            io.to(roomId).emit('playRightCardCli', {
                playedCardNumber: card,
                playedPlayer: playerId
            });
        }
    }

  // 커밋: 작업용 -> 실제
    for (const p of room.players) {
        room.cards[p] = room.workCards[p];
    }

    // 스테이지 클리어 체크(네 기존 함수 재사용)
    checkStageClear(io, roomId, remainingCardCount);
}