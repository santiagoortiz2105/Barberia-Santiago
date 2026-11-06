import axios from 'axios';
import { obtener_api_url } from '../config/debug';

const getBaseUrl = () => {
  const apiUrl = obtener_api_url();
  return (apiUrl || 'http://192.168.100.137:5211').replace(/\/+$/, '').replace(/\/api$/, '');
};

export const authService = {
  async loginConGoogle(idTokenGoogle: string) {
    const response = await axios.post(`${getBaseUrl()}/api/auth/google`, {
      idToken: idTokenGoogle
    });
    return response.data;
  },

  async loginTradicional(email: string, password: string) {
    const response = await axios.post(`${getBaseUrl()}/api/auth/login`, {
      email,
      password
    });
    return response.data;
  },

  async registrarUsuario(nombre: string, email: string, password: string) {
    const response = await axios.post(`${getBaseUrl()}/api/auth/register`, {
      nombre,
      email,
      password
    });
    return response.data;
  },

  async obtenerTurnoPublico(tokenPublico: string) {
    const response = await axios.get(`${getBaseUrl()}/api/publico/turnos/${tokenPublico}`);
    return response.data;
  }
};