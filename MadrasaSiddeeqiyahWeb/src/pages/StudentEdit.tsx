import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { studentService } from '../services/studentService';
import type { StudentUpdateDto } from '../types/student';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';

const StudentEdit: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [fullName, setFullName] = useState('');
  const [fatherName, setFatherName] = useState('');
  const [phone, setPhone] = useState('');
  const [course, setCourse] = useState('');
  const [hostelRequired, setHostelRequired] = useState(false);
  const [status, setStatus] = useState<'Pending' | 'Approved' | 'Rejected'>('Pending');
  const [aadhaarNumber, setAadhaarNumber] = useState('');
  const [isAadhaarVerified, setIsAadhaarVerified] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!id) return;

    const loadStudent = async () => {
      setLoading(true);
      try {
        const student = await studentService.getById(Number(id));
        setFullName(student.fullName);
        setFatherName(student.fatherName);
        setPhone(student.phone);
        setCourse(student.course);
        setHostelRequired(student.hostelRequired);
        setStatus(student.status);
        setAadhaarNumber(student.aadhaarNumber || '');
        setIsAadhaarVerified(student.isAadhaarVerified || false);
      } catch (err) {
        console.error(err);
        setError('Unable to load student details.');
      } finally {
        setLoading(false);
      }
    };

    loadStudent();
  }, [id]);

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!id) return;

    setSaving(true);
    setError('');

    const studentUpdate: StudentUpdateDto = {
      fullName,
      fatherName,
      phone,
      course,
      hostelRequired,
      aadhaarNumber,
      isAadhaarVerified,
      status,
    };

    try {
      await studentService.update(Number(id), studentUpdate);
      navigate('/admin');
    } catch (err) {
      console.error(err);
      setError('Unable to save changes.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-10 px-4 md:px-10">
      <div className="mx-auto max-w-3xl">
        <div className="mb-6 flex items-center justify-between gap-4">
          <button
            onClick={() => navigate('/admin')}
            className="inline-flex items-center gap-2 rounded-2xl border border-gray-200 bg-white px-4 py-2 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
          >
            <ArrowLeft className="w-4 h-4" /> Back to Dashboard
          </button>
          <h1 className="text-2xl font-bold text-gray-900">Edit Student Application</h1>
        </div>

        <div className="rounded-3xl bg-white p-8 shadow-sm border border-gray-200">
          {error && (
            <div className="mb-4 rounded-xl bg-red-50 p-4 text-sm text-red-700">
              {error}
            </div>
          )}

          {loading ? (
            <div className="flex items-center justify-center py-20">
              <Loader2 className="h-10 w-10 animate-spin text-primary-600" />
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-6">
              <div className="grid gap-6 md:grid-cols-2">
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  Student Name
                  <input
                    type="text"
                    value={fullName}
                    onChange={(e) => setFullName(e.target.value)}
                    required
                    className="w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                  />
                </label>
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  Father Name
                  <input
                    type="text"
                    value={fatherName}
                    onChange={(e) => setFatherName(e.target.value)}
                    required
                    className="w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                  />
                </label>
              </div>

              <div className="grid gap-6 md:grid-cols-2">
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  Phone
                  <input
                    type="tel"
                    value={phone}
                    onChange={(e) => setPhone(e.target.value)}
                    required
                    className="w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                  />
                </label>
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  Course
                  <select
                    value={course}
                    onChange={(e) => setCourse(e.target.value)}
                    required
                    className="w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                  >
                    <option value="">Select a course</option>
                    <option value="Hifz">Hifz-ul-Quran</option>
                    <option value="Nazira">Nazira-ul-Quran</option>
                    <option value="Aalim">Aalim Course</option>
                    <option value="Basic">Basic Deeniyat</option>
                  </select>
                </label>
              </div>

              <div className="grid gap-6 md:grid-cols-2">
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  Status
                  <select
                    value={status}
                    onChange={(e) => setStatus(e.target.value as 'Pending' | 'Approved' | 'Rejected')}
                    className="w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                  >
                    <option value="Pending">Pending</option>
                    <option value="Approved">Approved</option>
                    <option value="Rejected">Rejected</option>
                  </select>
                </label>
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  Hostel Required
                  <div className="flex items-center gap-3">
                    <input
                      type="checkbox"
                      checked={hostelRequired}
                      onChange={(e) => setHostelRequired(e.target.checked)}
                      className="h-5 w-5 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                    />
                    <span className="text-sm text-gray-600">Student requires hostel accommodation</span>
                  </div>
                </label>
              </div>

              <div className="grid gap-6 md:grid-cols-2 pt-2 border-t border-gray-100">
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  Aadhaar Number
                  <input
                    type="text"
                    value={aadhaarNumber}
                    onChange={(e) => setAadhaarNumber(e.target.value)}
                    placeholder="XXXX-XXXX-1234"
                    className="w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none font-mono"
                  />
                </label>
                <label className="space-y-2 text-sm font-medium text-gray-700">
                  UIDAI Verification Status
                  <div className="flex items-center gap-3 pt-2">
                    <input
                      type="checkbox"
                      checked={isAadhaarVerified}
                      onChange={(e) => setIsAadhaarVerified(e.target.checked)}
                      className="h-5 w-5 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500"
                    />
                    <span className="text-sm font-semibold text-emerald-800">
                      {isAadhaarVerified ? '✓ Marked as UIDAI Verified' : 'Unverified / Pending Check'}
                    </span>
                  </div>
                </label>
              </div>

              <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-end">
                <button
                  type="button"
                  onClick={() => navigate('/admin')}
                  className="rounded-2xl border border-gray-200 bg-white px-6 py-3 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={saving}
                  className="inline-flex items-center justify-center gap-2 rounded-2xl bg-primary-600 px-6 py-3 text-sm font-semibold text-white transition hover:bg-primary-700 disabled:cursor-not-allowed disabled:bg-gray-400"
                >
                  <Save className="h-4 w-4" />
                  {saving ? 'Saving...' : 'Save Changes'}
                </button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};

export default StudentEdit;
