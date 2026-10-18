export interface ExpenseItem {
  id: number;
  title: string;
  category: string;
  amount: number;
  date: string;
  spentBy: string;
  receiptNumber: string;
  description: string;
  campusName: string;
}

export interface DonationItem {
  id: number;
  donorName: string;
  amount: number;
  date: string;
  purpose: string;
  receiptNumber: string;
  campusName: string;
}

export interface CategorySpending {
  category: string;
  amount: number;
  percentage: number;
}

export interface FinanceSummary {
  totalDonationsReceived: number;
  totalExpensesSpent: number;
  netBalance: number;
  annualBudgetGoal: number;
  remainingBudgetNeeded: number;
  categoryBreakdown: CategorySpending[];
  recentExpenses: ExpenseItem[];
  recentDonations: DonationItem[];
  totalCampuses: number;
  totalStudentsEnrolled: number;
  totalCapacity: number;
}

export interface CampusItem {
  id: number;
  name: string;
  branchCode: string;
  address: string;
  city: string;
  state: string;
  latitude: number;
  longitude: number;
  googleMapEmbedUrl: string;
  studentCapacity: number;
  currentStudents: number;
  annualBudget: number;
  totalSpent: number;
  totalReceived: number;
  inchargePerson: string;
  contactPhone: string;
  capacityUtilizationPercentage: number;
  remainingBudget: number;
}
