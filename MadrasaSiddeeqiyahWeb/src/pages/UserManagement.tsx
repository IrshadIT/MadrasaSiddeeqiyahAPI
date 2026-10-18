import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { userService } from '../services/userService';
import { authService } from '../services/authService';
import type { User, UserCreateDto } from '../types/user';
import { UserPlus, PlusCircle, Trash2, ShieldCheck, ArrowLeft } from 'lucide-react';

const UserManagement: React.FC = () => {
  const navigate = useNavigate();
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState<'Admin' | 'User'>('User');
  const [saving, setSaving] = useState(false);

  const currentUser = authService.getCurrentUser();

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    setLoading(true);
    try {
      const usersData = await userService.getAll();
      setUsers(usersData);
      setError('');
    } catch (err) {
      console.error(err);
      setError('Unable to load user accounts.');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateUser = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!username || !password) {
      setError('Username and password are required.');
      return;
    }

    setSaving(true);
    setError('');

    const payload: UserCreateDto = { username, password, role };
    try {
      await userService.create(payload);
      setUsername('');
      setPassword('');
      setRole('User');
      loadUsers();
    } catch (err) {
      console.error(err);
      setError('Failed to create user. Username may already exist.');
    } finally {
      setSaving(false);
    }
  };

  const handleRoleChange = async (id: number, newRole: 'Admin' | 'User') => {
    try {
      await userService.update(id, { role: newRole });
      loadUsers();
    } catch (err) {
      console.error(err);
      setError('Unable to update user role.');
    }
  };

  const handleDelete = async (id: number, usernameToDelete: string) => {
    if (usernameToDelete === currentUser?.username) {
      setError('You cannot delete your own account while logged in.');
      return;
    }

    if (!window.confirm(`Delete account ${usernameToDelete}?`)) {
      return;
    }

    try {
      await userService.delete(id);
      loadUsers();
    } catch (err) {
      console.error(err);
      setError('Unable to delete user account.');
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 py-10 px-4 md:px-10">
      <div className="mx-auto max-w-6xl">
        <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">User Management</h1>
            <p className="text-sm text-gray-500">Create and manage portal users, roles, and access control.</p>
          </div>
          <button
            onClick={() => navigate('/admin')}
            className="inline-flex items-center gap-2 rounded-2xl border border-gray-200 bg-white px-4 py-3 text-sm font-semibold text-gray-700 transition hover:bg-gray-50"
          >
            <ArrowLeft className="w-4 h-4" /> Back to Admin
          </button>
        </div>

        {error && (
          <div className="mb-6 rounded-3xl bg-red-50 p-4 text-sm text-red-700 border border-red-200">
            {error}
          </div>
        )}

        <div className="grid gap-6 xl:grid-cols-[1.25fr_1fr]">
          <section className="rounded-3xl bg-white p-6 shadow-sm border border-gray-200">
            <div className="mb-6 flex items-center gap-3 text-gray-700">
              <ShieldCheck className="h-6 w-6 text-primary-600" />
              <div>
                <h2 className="text-lg font-semibold">Account Directory</h2>
                <p className="text-sm text-gray-500">Review active users and assign roles.</p>
              </div>
            </div>

            {loading ? (
              <div className="py-20 text-center text-gray-500">Loading users...</div>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full text-left border-separate border-spacing-y-2">
                  <thead>
                    <tr className="text-xs uppercase tracking-wide text-gray-500">
                      <th className="px-4 py-3">Username</th>
                      <th className="px-4 py-3">Role</th>
                      <th className="px-4 py-3">Created</th>
                      <th className="px-4 py-3">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((user) => (
                      <tr key={user.id} className="bg-slate-50 rounded-3xl shadow-sm mb-2 align-top">
                        <td className="px-4 py-4 text-sm font-semibold text-gray-900">{user.username}</td>
                        <td className="px-4 py-4">
                          <select
                            value={user.role}
                            onChange={(e) => handleRoleChange(user.id, e.target.value as 'Admin' | 'User')}
                            disabled={user.username === currentUser?.username}
                            className="rounded-2xl border border-gray-200 bg-white px-3 py-2 text-sm focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                          >
                            <option value="Admin">Admin</option>
                            <option value="User">User</option>
                          </select>
                        </td>
                        <td className="px-4 py-4 text-sm text-gray-500">{new Date(user.createdAt).toLocaleDateString()}</td>
                        <td className="px-4 py-4 flex flex-wrap gap-2">
                          <button
                            onClick={() => handleDelete(user.id, user.username)}
                            disabled={user.username === currentUser?.username}
                            className="inline-flex items-center gap-2 rounded-2xl border border-red-200 bg-red-50 px-3 py-2 text-sm font-semibold text-red-700 transition hover:bg-red-100 disabled:cursor-not-allowed disabled:opacity-50"
                          >
                            <Trash2 className="h-4 w-4" /> Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>

          <section className="rounded-3xl bg-white p-6 shadow-sm border border-gray-200">
            <div className="mb-6 flex items-center gap-3 text-gray-700">
              <UserPlus className="h-6 w-6 text-primary-600" />
              <div>
                <h2 className="text-lg font-semibold">Create New User</h2>
                <p className="text-sm text-gray-500">Add a portal user with credentials and assign a role.</p>
              </div>
            </div>

            <form className="space-y-5" onSubmit={handleCreateUser}>
              <div>
                <label className="block text-sm font-semibold text-gray-700">Username</label>
                <input
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  className="mt-2 w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                  placeholder="new.user"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700">Password</label>
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="mt-2 w-full rounded-2xl border border-gray-200 px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                  placeholder="Secure password"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-semibold text-gray-700">Role</label>
                <select
                  value={role}
                  onChange={(e) => setRole(e.target.value as 'Admin' | 'User')}
                  className="mt-2 w-full rounded-2xl border border-gray-200 bg-white px-4 py-3 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 outline-none"
                >
                  <option value="User">User</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>

              <button
                type="submit"
                disabled={saving}
                className="inline-flex items-center justify-center gap-2 rounded-2xl bg-primary-600 px-6 py-3 text-sm font-semibold text-white transition hover:bg-primary-700 disabled:cursor-not-allowed disabled:bg-gray-400"
              >
                <PlusCircle className="h-4 w-4" />
                {saving ? 'Creating...' : 'Create Account'}
              </button>
            </form>
          </section>
        </div>
      </div>
    </div>
  );
};

export default UserManagement;
