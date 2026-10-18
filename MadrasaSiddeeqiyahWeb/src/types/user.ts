export interface User {
  id: number;
  username: string;
  role: 'Admin' | 'User';
  createdAt: string;
}

export interface UserCreateDto {
  username: string;
  password: string;
  role: 'Admin' | 'User';
}

export interface UserUpdateDto {
  password?: string;
  role?: 'Admin' | 'User';
}
