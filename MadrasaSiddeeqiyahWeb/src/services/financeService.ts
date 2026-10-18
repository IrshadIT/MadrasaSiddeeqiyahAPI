import api from './api';
import type { FinanceSummary, ExpenseItem, DonationItem, CampusItem } from '../types/finance';

export const financeService = {
  getSummary: async (): Promise<FinanceSummary> => {
    const response = await api.get<FinanceSummary>('/finance/summary');
    return response.data;
  },
  getExpenses: async (category?: string, campus?: string): Promise<ExpenseItem[]> => {
    const response = await api.get<ExpenseItem[]>('/finance/expenses', {
      params: { category, campus }
    });
    return response.data;
  },
  getDonations: async (): Promise<DonationItem[]> => {
    const response = await api.get<DonationItem[]>('/finance/donations');
    return response.data;
  },
  getCampuses: async (): Promise<CampusItem[]> => {
    const response = await api.get<CampusItem[]>('/campus');
    return response.data;
  },
  getCampusById: async (id: number): Promise<CampusItem> => {
    const response = await api.get<CampusItem>(`/campus/${id}`);
    return response.data;
  },
  createExpense: async (data: Partial<ExpenseItem>): Promise<ExpenseItem> => {
    const response = await api.post<ExpenseItem>('/finance/expenses', data);
    return response.data;
  },
  createDonation: async (data: Partial<DonationItem>): Promise<DonationItem> => {
    const response = await api.post<DonationItem>('/finance/donations', data);
    return response.data;
  }
};
