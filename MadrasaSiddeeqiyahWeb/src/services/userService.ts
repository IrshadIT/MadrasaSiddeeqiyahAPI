import api from './api';
import type { User, UserCreateDto, UserUpdateDto } from '../types/user';

export const userService = {
  getAll: async () => {
    const response = await api.get<User[]>('/users');
    return response.data;
  },
  create: async (user: UserCreateDto) => {
    const response = await api.post<User>('/users', user);
    return response.data;
  },
  update: async (id: number, user: UserUpdateDto) => {
    await api.put(`/users/${id}`, user);
  },
  delete: async (id: number) => {
    await api.delete(`/users/${id}`);
  },
};
