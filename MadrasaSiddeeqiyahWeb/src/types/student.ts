export interface Student {
  id: number;
  fullName: string;
  fatherName: string;
  phone: string;
  course: string;
  hostelRequired: boolean;
  aadhaarNumber?: string;
  isAadhaarVerified?: boolean;
  aadhaarVerifiedAt?: string | null;
  applicationDate: string;
  status: 'Pending' | 'Approved' | 'Rejected';
}

export interface StudentCreateDto {
  fullName: string;
  fatherName: string;
  phone: string;
  course: string;
  hostelRequired: boolean;
  aadhaarNumber?: string;
  isAadhaarVerified?: boolean;
  aadhaarVerifiedAt?: string | null;
}

export interface StudentUpdateDto {
  fullName: string;
  fatherName: string;
  phone: string;
  course: string;
  hostelRequired: boolean;
  aadhaarNumber?: string;
  isAadhaarVerified?: boolean;
  aadhaarVerifiedAt?: string | null;
  status: 'Pending' | 'Approved' | 'Rejected';
}

export interface StudentStatusUpdateDto {
  status: string;
}

export interface StudentQueryParams {
  searchTerm?: string;
  status?: string;
  course?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface StudentSummary {
  totalApplications: number;
  pending: number;
  approved: number;
  rejected: number;
}

export interface LoginResponse {
  token: string;
}

export interface AadhaarVerifyResponse {
  isValid: boolean;
  isVerified: boolean;
  maskedAadhaar: string;
  message: string;
  verifiedAt: string;
}

export interface Course {
  code: string;
  name: string;
  duration: string;
  description: string;
  hostelAvailable: boolean;
}
