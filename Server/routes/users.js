var express = require('express');
var router = express.Router();

const roomController = require('../controllers/roomController');

router.post('/createRoom', roomController.createRoom);
router.post('/joinRoom', roomController.joinRoom);
router.post('/destroyRoom', roomController.destroyRoom);

module.exports = router;