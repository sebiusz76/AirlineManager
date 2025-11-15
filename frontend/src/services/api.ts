import axios from 'axios';

const api = axios.create({
	baseURL: 'http://localhost:5000/api', // lub proxy w dev
});

export default api;
