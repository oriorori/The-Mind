const express = require('express');
const router = express.Router();

// 👥 간단한 대기열
let queue = [];
let rooms = [];
let roomId = 1;
const MAX_PLAYERS_PER_ROOM = 2;

// 🧑‍🤝‍🧑 매치 요청 (POST)
router.post('/join', (req, res) => {
  const { playerId } = req.body;

  if (!playerId) return res.status(400).json({ error: 'playerId is required' });

  console.log(`🟡 ${playerId} 매치 대기열에 추가됨`);
  queue.push(playerId);

  // 플레이어가 MAX명 이상 모이면 매치 성사
  if (queue.length >= MAX_PLAYERS_PER_ROOM) {
    const players = queue.splice(0, MAX_PLAYERS_PER_ROOM);
    const newRoom = {
      id: roomId++,
      players,
    };
    rooms.push(newRoom);

    console.log(`🟢 방 #${newRoom.id} 생성됨: ${players.join(', ')}`);

    return res.json({
      matched: true,
      room: newRoom,
    });
  }

  return res.json({
    matched: false,
    message: '매치 대기 중...',
  });
});

// 방 목록 보기 (디버깅용)
router.get('/rooms', (req, res) => {
  res.json(rooms);
});

module.exports = router;