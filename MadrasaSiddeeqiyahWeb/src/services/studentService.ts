import api from './api';
import type {
  Student,
  StudentCreateDto,
  StudentStatusUpdateDto,
  StudentUpdateDto,
  StudentQueryParams,
  PagedResult,
  StudentSummary,
} from '../types/student';

export const studentService = {
  getAll: async (params: StudentQueryParams) => {
    const response = await api.get<PagedResult<Student>>('/student', { params });
    return response.data;
  },
  getById: async (id: number) => {
    const response = await api.get<Student>(`/student/${id}`);
    return response.data;
  },
  create: async (student: StudentCreateDto) => {
    const response = await api.post<Student>('/student', student);
    return response.data;
  },
  update: async (id: number, student: StudentUpdateDto) => {
    await api.put(`/student/${id}`, student);
  },
  updateStatus: async (id: number, status: StudentStatusUpdateDto) => {
    await api.put(`/student/${id}/status`, status);
  },
  delete: async (id: number) => {
    await api.delete(`/student/${id}`);
  },
  getSummary: async () => {
    const response = await api.get<StudentSummary>('/student/summary');
    return response.data;
  },
  exportCsv: async (params: StudentQueryParams) => {
    const response = await api.get('/student/export', {
      params,
      responseType: 'blob',
    });
    return response.data;
  },
  verifyAadhaar: async (aadhaarNumber: string, fullName?: string) => {
    const response = await api.post('/student/verify-aadhaar', { aadhaarNumber, fullName: fullName || '' });
    return response.data;
  },
  getCourses: async () => {
    const response = await api.get('/student/courses');
    return response.data;
  },
  bulkUpdateStatus: async (studentIds: number[], status: string) => {
    const response = await api.post('/student/bulk-status', { studentIds, status });
    return response.data;
  },
};
