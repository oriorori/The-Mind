// 방 저장소
let rooms = {};

async function createRoom(req, res) {
  try {
    const { playerId, roomId, roomSize } = req.body;

    if(!playerId || !roomId || !roomSize) {
      return res.status(400).json({ error: '필수 값 없음' });
    }

    if(rooms[roomId]) {
      return res.status(409).json({ error: '방이 이미 존재함' });
    }

    rooms[roomId] = {
      id: roomId,
      players: [playerId],
      roomSize: roomSize,
      playerCount: 1
    };

    console.log(`🟢 ${playerId}님이 방 #${roomId}을 만들었어요`);
    return res.json({ room: rooms[roomId] });
  }

  catch (error) {
    console.error('방 생성 에러:', error);
    return res.status(500).json({ error: 'Internal server error' });
  }
}

async function joinRoom(req, res){
  try {
    const { playerId, roomId } = req.body;

    if (!playerId || !roomId) {
      return res.status(400).json({ error: '필수 값 없음' });
    }

    const room = rooms[roomId];
    if (!room) {
      return res.status(404).json({ error: '방이 존재하지 않음' });
    }

    if (room.players.length >= room.roomSize) {
      return res.status(403).json({ error: '방이 가득참' });
    }

    if (room.players.includes(playerId)) {
      return res.status(409).json({ error: '이미 방에 참가함' });
    }

    room.players.push(playerId);
    room.playerCount += 1;
    console.log(`✅ ${playerId}님이 방 #${roomId}에 입장했어요`);
    return res.json({ room });
  } catch (error) {
    console.error('방 참가 에러:', error);
    return res.status(500).json({ error: 'Internal server error' });
  }
}

async function destroyRoom(roomId) {
  try {
    if (!rooms[roomId]) {
      console.error(`방 #${roomId}이 존재하지 않습니다.`);
      return;
    }
    delete rooms[roomId];
    console.log(`🟢 방 #${roomId}이 삭제되었습니다.`);
  } catch (error) {
    console.error('방 삭제 에러:', error);
  }
}

module.exports = {
  createRoom,
  joinRoom,
  destroyRoom,
};