import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5219/api', // patikrinkite, ar portas teisingas
});

export default api;