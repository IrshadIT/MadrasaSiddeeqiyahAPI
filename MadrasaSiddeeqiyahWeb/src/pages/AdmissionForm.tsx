import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { studentService } from '../services/studentService';
import type { StudentCreateDto, Course } from '../types/student';
import { 
  GraduationCap, 
  Phone, 
  User, 
  BookOpen, 
  Send, 
  LogIn, 
  Fingerprint, 
  CheckCircle2, 
  AlertCircle, 
  Sparkles, 
  ShieldCheck,
  Building2,
  Loader2
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const DEFAULT_COURSES: Course[] = [
  { code: 'Hifz', name: 'Hifz-ul-Quran', duration: '3 - 4 Years', description: 'Complete Quran memorization with Tajweed', hostelAvailable: true },
  { code: 'Nazira', name: 'Nazira-ul-Quran', duration: '1 Year', description: 'Accurate Quran recitation & basic supplications', hostelAvailable: false },
  { code: 'Aalim', name: 'Aalim Course', duration: '6 - 7 Years', description: 'Classical Islamic scholarship & Arabic theology', hostelAvailable: true },
  { code: 'Basic', name: 'Basic Deeniyat', duration: '6 Months', description: 'Foundational Salah, Duas & Islamic ethics', hostelAvailable: false }
];

const AdmissionForm: React.FC = () => {
  const navigate = useNavigate();
  const [courses, setCourses] = useState<Course[]>(DEFAULT_COURSES);
  const [formData, setFormData] = useState<StudentCreateDto>({
    fullName: '',
    fatherName: '',
    phone: '',
    course: '',
    hostelRequired: false,
    aadhaarNumber: '',
    isAadhaarVerified: false,
    aadhaarVerifiedAt: null,
  });

  const [rawAadhaar, setRawAadhaar] = useState('');
  const [aadhaarVerifying, setAadhaarVerifying] = useState(false);
  const [aadhaarStatus, setAadhaarStatus] = useState<{
    verified: boolean;
    message?: string;
    masked?: string;
    error?: boolean;
  } | null>(null);

  const [submitting, setSubmitting] = useState(false);
  const [submitted, setSubmitted] = useState(false);
  const [lastSubmittedStudent, setLastSubmittedStudent] = useState<any>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    studentService.getCourses()
      .then(res => {
        if (Array.isArray(res) && res.length > 0) setCourses(res);
      })
      .catch(() => {
        // Fallback to DEFAULT_COURSES
      });
  }, []);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? (e.target as HTMLInputElement).checked : value
    }));
  };

  // Format Aadhaar into 4-digit groups (XXXX XXXX XXXX)
  const handleAadhaarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const raw = e.target.value.replace(/\D/g, '').slice(0, 12);
    setRawAadhaar(raw);

    // Format with spaces
    const formatted = raw.replace(/(\d{4})(?=\d)/g, '$1 ');
    setFormData(prev => ({
      ...prev,
      aadhaarNumber: formatted,
      isAadhaarVerified: false,
      aadhaarVerifiedAt: null
    }));
    setAadhaarStatus(null);
  };

  const handleVerifyAadhaar = async () => {
    if (!rawAadhaar || rawAadhaar.length !== 12) {
      setAadhaarStatus({
        verified: false,
        error: true,
        message: 'Aadhaar must be exactly 12 numeric digits.'
      });
      return;
    }

    setAadhaarVerifying(true);
    setAadhaarStatus(null);

    try {
      const res = await studentService.verifyAadhaar(rawAadhaar, formData.fullName);
      if (res.isValid) {
        setFormData(prev => ({
          ...prev,
          aadhaarNumber: res.maskedAadhaar,
          isAadhaarVerified: true,
          aadhaarVerifiedAt: res.verifiedAt
        }));
        setAadhaarStatus({
          verified: true,
          masked: res.maskedAadhaar,
          message: 'UIDAI Verhoeff Checksum Verified'
        });
      } else {
        setAadhaarStatus({
          verified: false,
          error: true,
          message: res.message || 'Aadhaar verification failed.'
        });
      }
    } catch (err: any) {
      setAadhaarStatus({
        verified: false,
        error: true,
        message: err.response?.data?.message || 'Invalid Aadhaar checksum or verification error.'
      });
    } finally {
      setAadhaarVerifying(false);
    }
  };

  const handleQuickSampleAadhaar = () => {
    const sample = '234567890124';
    setRawAadhaar(sample);
    const formatted = '2345 6789 0124';
    setFormData(prev => ({
      ...prev,
      aadhaarNumber: formatted,
      isAadhaarVerified: false,
      aadhaarVerifiedAt: null
    }));
    setAadhaarStatus(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');

    try {
      const payload: StudentCreateDto = {
        ...formData,
        aadhaarNumber: rawAadhaar ? rawAadhaar : formData.aadhaarNumber,
      };

      const result = await studentService.create(payload);
      setLastSubmittedStudent(result);
      setSubmitted(true);
    } catch (err: any) {
      if (err.response?.data?.errors) {
        const errorMessages = Object.values(err.response.data.errors).flat().join(' ');
        setError(errorMessages);
      } else if (err.response?.data?.message) {
        setError(err.response.data.message);
      } else {
        setError('Failed to submit application. Please check your details and try again.');
      }
      console.error(err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleReset = () => {
    setFormData({
      fullName: '',
      fatherName: '',
      phone: '',
      course: '',
      hostelRequired: false,
      aadhaarNumber: '',
      isAadhaarVerified: false,
      aadhaarVerifiedAt: null,
    });
    setRawAadhaar('');
    setAadhaarStatus(null);
    setSubmitted(false);
  };

  if (submitted) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-slate-50 via-primary-50/20 to-emerald-50/30 flex items-center justify-center p-4">
        <motion.div 
          initial={{ opacity: 0, scale: 0.9, y: 20 }}
          animate={{ opacity: 1, scale: 1, y: 0 }}
          transition={{ type: 'spring', damping: 25, stiffness: 260 }}
          className="bg-white p-8 sm:p-10 rounded-3xl shadow-2xl max-w-lg w-full border border-emerald-100 text-center relative overflow-hidden"
        >
          <div className="absolute top-0 left-0 right-0 h-2 bg-gradient-to-r from-primary-500 via-emerald-500 to-teal-500" />
          
          <motion.div 
            initial={{ scale: 0 }}
            animate={{ scale: 1 }}
            transition={{ type: 'spring', delay: 0.15, stiffness: 300, damping: 20 }}
            className="w-20 h-20 bg-emerald-100 text-emerald-600 rounded-3xl flex items-center justify-center mx-auto mb-6 shadow-inner"
          >
            <CheckCircle2 className="w-10 h-10" />
          </motion.div>

          <h2 className="text-3xl font-black text-gray-900 tracking-tight mb-2">
            Application Received!
          </h2>
          <p className="text-gray-600 text-sm leading-relaxed mb-6">
            Congratulations! The admission application for{' '}
            <strong className="text-gray-900 font-semibold">{lastSubmittedStudent?.fullName || formData.fullName}</strong>{' '}
            has been registered in the system.
          </p>

          <div className="bg-slate-50 p-4 rounded-2xl border border-slate-200 text-left text-xs space-y-2 mb-8">
            <div className="flex justify-between py-1 border-b border-slate-100">
              <span className="text-gray-500">Application ID:</span>
              <span className="font-mono font-bold text-gray-900">#{lastSubmittedStudent?.id || 'NEW'}</span>
            </div>
            <div className="flex justify-between py-1 border-b border-slate-100">
              <span className="text-gray-500">Selected Course:</span>
              <span className="font-semibold text-primary-700">{formData.course}</span>
            </div>
            <div className="flex justify-between py-1 border-b border-slate-100">
              <span className="text-gray-500">Contact Phone:</span>
              <span className="font-mono text-gray-900">+91 {formData.phone}</span>
            </div>
            <div className="flex justify-between py-1">
              <span className="text-gray-500">Aadhaar Status:</span>
              <span className="inline-flex items-center gap-1 font-semibold text-emerald-700">
                <ShieldCheck className="w-3.5 h-3.5" />
                {lastSubmittedStudent?.aadhaarNumber ? lastSubmittedStudent.aadhaarNumber : 'Verified'}
              </span>
            </div>
          </div>

          <motion.button 
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
            onClick={handleReset}
            className="w-full bg-gradient-to-r from-primary-600 to-primary-700 hover:from-primary-700 hover:to-primary-800 text-white font-bold py-3.5 px-6 rounded-2xl shadow-lg shadow-primary-200 transition"
          >
            Submit Another Admission
          </motion.button>
        </motion.div>
      </div>
    );
  }

  const selectedCourseObj = courses.find(c => c.code === formData.course);

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 via-white to-primary-50/30 flex flex-col antialiased selection:bg-primary-500 selection:text-white">
      {/* Sleek Top Navigation Bar */}
      <header className="bg-white/80 backdrop-blur-md border-b border-gray-200/80 sticky top-0 z-50">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 h-18 flex justify-between items-center">
          <div className="flex items-center gap-3.5">
            <motion.div 
              whileHover={{ rotate: 10, scale: 1.05 }}
              className="p-2.5 bg-gradient-to-br from-primary-500 to-primary-700 text-white rounded-2xl shadow-md shadow-primary-200 flex items-center justify-center"
            >
              <GraduationCap className="w-6 h-6" />
            </motion.div>
            <div>
              <div className="flex items-center gap-2">
                <span className="font-black text-gray-900 tracking-tight text-lg sm:text-xl">Madrasa Siddeeqiyah</span>
                <span className="hidden sm:inline-flex items-center gap-1 text-[11px] font-bold bg-primary-50 text-primary-700 px-2.5 py-0.5 rounded-full border border-primary-200/60">
                  <Sparkles className="w-3 h-3 text-primary-500" /> Admissions 2026
                </span>
              </div>
              <p className="text-[11px] text-gray-500 font-medium">Centre of Islamic Learning & Quranic Studies</p>
            </div>
          </div>

          <motion.button
            whileHover={{ scale: 1.03 }}
            whileTap={{ scale: 0.97 }}
            type="button"
            onClick={() => navigate('/login')}
            className="inline-flex items-center gap-2 rounded-2xl border border-gray-300/80 bg-white px-4 py-2 text-xs sm:text-sm font-bold text-gray-700 shadow-xs hover:border-primary-400 hover:text-primary-600 transition"
          >
            <LogIn className="w-4 h-4 text-primary-600" /> Admin Portal
          </motion.button>
        </div>
      </header>

      {/* Main Container */}
      <main className="flex-1 py-10 sm:py-14 px-4 sm:px-6 lg:px-8">
        <motion.div 
          initial={{ opacity: 0, y: 30 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.6, ease: [0.16, 1, 0.3, 1] }}
          className="max-w-2xl mx-auto"
        >
          {/* Header Title */}
          <div className="text-center mb-10">
            <span className="inline-block text-xs uppercase tracking-widest font-extrabold text-primary-600 bg-primary-100/70 px-3.5 py-1 rounded-full mb-3">
              Online Registration
            </span>
            <h1 className="text-3xl sm:text-5xl font-black text-gray-900 tracking-tight">
              Student Admission
            </h1>
            <p className="mt-3 text-sm sm:text-base text-gray-600 max-w-lg mx-auto">
              Please complete all mandatory information and verify student Aadhaar for enrollment.
            </p>
          </div>

          {/* Form Card */}
          <div className="bg-white rounded-3xl shadow-xl shadow-slate-200/60 overflow-hidden border border-gray-100 transition">
            {/* Header Accent */}
            <div className="bg-gradient-to-r from-primary-700 via-primary-600 to-teal-600 py-6 px-8 text-white relative">
              <div className="flex justify-between items-center">
                <div>
                  <h2 className="text-xl font-bold tracking-tight">Admission Application Form</h2>
                  <p className="text-primary-100 text-xs mt-0.5">Academic Session 2026 - 2027</p>
                </div>
                <div className="p-2 bg-white/10 rounded-xl backdrop-blur-xs">
                  <ShieldCheck className="w-6 h-6 text-white" />
                </div>
              </div>
            </div>

            <form onSubmit={handleSubmit} className="p-6 sm:p-10 space-y-7">
              {error && (
                <motion.div 
                  initial={{ opacity: 0, height: 0 }}
                  animate={{ opacity: 1, height: 'auto' }}
                  className="bg-red-50 border-l-4 border-red-500 p-4 rounded-xl flex items-start gap-3"
                >
                  <AlertCircle className="w-5 h-5 text-red-600 shrink-0 mt-0.5" />
                  <p className="text-sm font-medium text-red-700">{error}</p>
                </motion.div>
              )}

              {/* Personal Details */}
              <div className="space-y-4">
                <h3 className="text-xs uppercase tracking-wider font-extrabold text-gray-400">
                  1. Candidate Particulars
                </h3>

                <div className="grid grid-cols-1 gap-5 sm:grid-cols-2">
                  <div className="space-y-1.5">
                    <label className="text-xs font-bold uppercase tracking-wider text-gray-700 flex items-center gap-1.5">
                      <User className="w-3.5 h-3.5 text-primary-500" /> Full Name <span className="text-red-500">*</span>
                    </label>
                    <input
                      required
                      type="text"
                      name="fullName"
                      value={formData.fullName}
                      onChange={handleChange}
                      className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none transition text-sm bg-gray-50/50 focus:bg-white"
                      placeholder="e.g. Zayd Khan"
                    />
                  </div>

                  <div className="space-y-1.5">
                    <label className="text-xs font-bold uppercase tracking-wider text-gray-700 flex items-center gap-1.5">
                      <User className="w-3.5 h-3.5 text-primary-500" /> Father's Name <span className="text-red-500">*</span>
                    </label>
                    <input
                      required
                      type="text"
                      name="fatherName"
                      value={formData.fatherName}
                      onChange={handleChange}
                      className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none transition text-sm bg-gray-50/50 focus:bg-white"
                      placeholder="e.g. Farooq Khan"
                    />
                  </div>
                </div>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-700 flex items-center gap-1.5">
                    <Phone className="w-3.5 h-3.5 text-primary-500" /> Phone Number (10 Digits) <span className="text-red-500">*</span>
                  </label>
                  <input
                    required
                    type="tel"
                    name="phone"
                    maxLength={10}
                    pattern="\d{10}"
                    value={formData.phone}
                    onChange={handleChange}
                    className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none transition text-sm bg-gray-50/50 focus:bg-white font-mono"
                    placeholder="9876543210"
                  />
                </div>
              </div>

              {/* Aadhaar Verification Card */}
              <div className="space-y-3 pt-2">
                <div className="flex items-center justify-between">
                  <h3 className="text-xs uppercase tracking-wider font-extrabold text-gray-400 flex items-center gap-1.5">
                    2. UIDAI Aadhaar Verification
                  </h3>
                  <button
                    type="button"
                    onClick={handleQuickSampleAadhaar}
                    className="text-[11px] font-bold text-primary-600 hover:text-primary-800 transition underline underline-offset-2"
                  >
                    Use Sample Valid Aadhaar
                  </button>
                </div>

                <div className="p-5 rounded-2xl border-2 border-dashed border-primary-200 bg-primary-50/30 space-y-4">
                  <div className="flex flex-col sm:flex-row gap-3">
                    <div className="relative flex-1">
                      <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-primary-600">
                        <Fingerprint className="w-5 h-5" />
                      </div>
                      <input
                        type="text"
                        name="aadhaarInput"
                        maxLength={14}
                        value={formData.aadhaarNumber}
                        onChange={handleAadhaarChange}
                        className="w-full pl-11 pr-4 py-3 rounded-xl border border-gray-300 focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none font-mono tracking-wider text-base bg-white shadow-xs"
                        placeholder="XXXX XXXX XXXX"
                      />
                    </div>

                    <motion.button
                      whileHover={{ scale: 1.02 }}
                      whileTap={{ scale: 0.98 }}
                      type="button"
                      disabled={aadhaarVerifying || rawAadhaar.length !== 12 || (aadhaarStatus?.verified ?? false)}
                      onClick={handleVerifyAadhaar}
                      className={`px-5 py-3 rounded-xl text-xs font-bold flex items-center justify-center gap-2 transition shadow-sm ${
                        aadhaarStatus?.verified
                          ? 'bg-emerald-600 text-white cursor-default'
                          : rawAadhaar.length === 12
                          ? 'bg-primary-600 hover:bg-primary-700 text-white shadow-primary-200'
                          : 'bg-gray-200 text-gray-400 cursor-not-allowed'
                      }`}
                    >
                      {aadhaarVerifying ? (
                        <>
                          <Loader2 className="w-4 h-4 animate-spin" /> Verifying...
                        </>
                      ) : aadhaarStatus?.verified ? (
                        <>
                          <CheckCircle2 className="w-4 h-4" /> Verified
                        </>
                      ) : (
                        <>
                          <ShieldCheck className="w-4 h-4" /> Verify Aadhaar
                        </>
                      )}
                    </motion.button>
                  </div>

                  {/* Verification Feedback Banner */}
                  <AnimatePresence>
                    {aadhaarStatus && (
                      <motion.div
                        initial={{ opacity: 0, height: 0 }}
                        animate={{ opacity: 1, height: 'auto' }}
                        exit={{ opacity: 0, height: 0 }}
                        className={`p-3.5 rounded-xl text-xs flex items-center gap-2.5 font-medium ${
                          aadhaarStatus.verified
                            ? 'bg-emerald-50 text-emerald-800 border border-emerald-200'
                            : 'bg-rose-50 text-rose-800 border border-rose-200'
                        }`}
                      >
                        {aadhaarStatus.verified ? (
                          <>
                            <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0" />
                            <div className="flex-1">
                              <span><strong>{aadhaarStatus.masked}</strong> &mdash; {aadhaarStatus.message}</span>
                            </div>
                          </>
                        ) : (
                          <>
                            <AlertCircle className="w-4 h-4 text-rose-600 shrink-0" />
                            <div className="flex-1">
                              <span>{aadhaarStatus.message}</span>
                            </div>
                          </>
                        )}
                      </motion.div>
                    )}
                  </AnimatePresence>

                  <p className="text-[11px] text-gray-500">
                    Aadhaar digits are checked against the official UIDAI Verhoeff algorithm. Masked details (<span className="font-mono">XXXX-XXXX-1234</span>) are safely stored in the database.
                  </p>
                </div>
              </div>

              {/* Course Selection */}
              <div className="space-y-3 pt-2">
                <h3 className="text-xs uppercase tracking-wider font-extrabold text-gray-400">
                  3. Course & Accommodation
                </h3>

                <div className="space-y-1.5">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-700 flex items-center gap-1.5">
                    <BookOpen className="w-3.5 h-3.5 text-primary-500" /> Interested Curriculum <span className="text-red-500">*</span>
                  </label>
                  <select
                    required
                    name="course"
                    value={formData.course}
                    onChange={handleChange}
                    className="w-full px-4 py-3 rounded-2xl border border-gray-200 focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none transition text-sm bg-gray-50/50 focus:bg-white"
                  >
                    <option value="">Select a Course</option>
                    {courses.map(c => (
                      <option key={c.code} value={c.code}>
                        {c.name} &mdash; ({c.duration})
                      </option>
                    ))}
                  </select>
                </div>

                {selectedCourseObj && (
                  <motion.div 
                    initial={{ opacity: 0, y: -5 }}
                    animate={{ opacity: 1, y: 0 }}
                    className="p-3.5 bg-blue-50/80 rounded-2xl border border-blue-100 text-xs text-blue-900 space-y-1"
                  >
                    <div className="font-bold text-blue-800">{selectedCourseObj.name}</div>
                    <p className="text-blue-700">{selectedCourseObj.description}</p>
                    <div className="flex gap-4 pt-1 font-semibold text-[11px]">
                      <span>Duration: {selectedCourseObj.duration}</span>
                      <span>Hostel: {selectedCourseObj.hostelAvailable ? 'Available' : 'Day Scholars'}</span>
                    </div>
                  </motion.div>
                )}

                <div className="flex items-center gap-3.5 p-4 bg-slate-50 rounded-2xl border border-slate-200/80 transition hover:bg-slate-100/70">
                  <input
                    id="hostelRequired"
                    name="hostelRequired"
                    type="checkbox"
                    checked={formData.hostelRequired}
                    onChange={handleChange}
                    className="h-5 w-5 text-primary-600 focus:ring-primary-500 border-gray-300 rounded-lg cursor-pointer"
                  />
                  <label htmlFor="hostelRequired" className="text-sm font-semibold text-gray-800 cursor-pointer flex items-center gap-2">
                    <Building2 className="w-4 h-4 text-primary-600" />
                    Hostel & Boarding Accommodation Required
                  </label>
                </div>
              </div>

              {/* Submit Button */}
              <motion.button
                whileHover={{ scale: 1.01 }}
                whileTap={{ scale: 0.98 }}
                disabled={submitting}
                type="submit"
                className={`w-full flex items-center justify-center gap-2 py-4 px-6 rounded-2xl shadow-xl text-base font-extrabold text-white transition ${
                  submitting 
                  ? 'bg-gray-400 cursor-not-allowed' 
                  : 'bg-gradient-to-r from-primary-600 via-primary-700 to-teal-700 hover:shadow-primary-200'
                }`}
              >
                {submitting ? (
                  <>
                    <Loader2 className="w-5 h-5 animate-spin" /> Registering Student...
                  </>
                ) : (
                  <>
                    <Send className="w-5 h-5" /> Submit Admission Request
                  </>
                )}
              </motion.button>
            </form>
          </div>

          <footer className="mt-10 text-center text-xs text-gray-400 space-y-1">
            <p>&copy; 2026 Madrasa Siddeeqiyah. All rights reserved.</p>
            <p className="text-[11px] text-gray-400">UIDAI Verhoeff Checksum Verification Enabled &bull; Secure SQL Server Storage</p>
          </footer>
        </motion.div>
      </main>
    </div>
  );
};

export default AdmissionForm;
