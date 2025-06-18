function getRandomNumbers(count, min = 1, max = 100) {
  const numbers = [];
  while (numbers.length < count) {
    const n = Math.floor(Math.random() * (max - min + 1)) + min;
    numbers.push(n);
  }
  return numbers;
}