function createRoomInfo(roomSize) {
  return {
    players: [],
    roomSize: roomSize,
    waiting: false,
    currentStage: 0,
    remainingLife: 0,
    remainingShurikens: 0,
    shuffling: false,
    playing: false,
    gameStartVotes: new Set(),
    shurikenVotes: new Set(),
    cards: {}
  };
}

module.exports = {
  createRoomInfo
}