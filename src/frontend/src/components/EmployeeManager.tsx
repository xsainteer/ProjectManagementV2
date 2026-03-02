import React, { useState, useEffect } from 'react';
import { api } from '../api';
import type { Employee } from '../types';

export const EmployeeManager: React.FC = () => {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    middleName: '',
    email: ''
  });
  const [error, setError] = useState('');

  const fetchEmployees = async () => {
    const res = await api.employees.getAll();
    if (res.isSuccess) {
      setEmployees(res.value);
    }
  };

  useEffect(() => {
    fetchEmployees();
  }, []);

  const handleOpenCreate = () => {
    setEditingEmployee(null);
    setFormData({ firstName: '', lastName: '', middleName: '', email: '' });
    setIsFormOpen(true);
    setError('');
  };

  const handleOpenEdit = (employee: Employee) => {
    setEditingEmployee(employee);
    setFormData({
      firstName: employee.firstName,
      lastName: employee.lastName,
      middleName: employee.middleName || '',
      email: employee.email
    });
    setIsFormOpen(true);
    setError('');
  };

  const handleDelete = async (id: number) => {
    if (confirm('Are you sure you want to delete this employee?')) {
      const res = await api.employees.delete(id);
      if (res.isSuccess) {
        fetchEmployees();
      } else {
        alert(res.error?.message || 'Error deleting employee');
      }
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (editingEmployee) {
      const res = await api.employees.update(editingEmployee.id, {
        id: editingEmployee.id,
        ...formData
      });
      if (res.isSuccess) {
        setIsFormOpen(false);
        fetchEmployees();
      } else {
        setError(res.error?.message || 'Error updating employee');
      }
    } else {
      const res = await api.employees.create(formData);
      if (res.isSuccess) {
        setIsFormOpen(false);
        fetchEmployees();
      } else {
        setError(res.error?.message || 'Error creating employee');
      }
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h2>Employee Management</h2>
        <button className="primary" onClick={handleOpenCreate}>+ Add Employee</button>
      </div>

      {isFormOpen && (
        <div className="card" style={{ marginBottom: '2rem', maxWidth: '500px' }}>
          <h3>{editingEmployee ? 'Edit Employee' : 'New Employee'}</h3>
          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>First Name</label>
              <input 
                type="text" 
                required 
                value={formData.firstName} 
                onChange={e => setFormData({ ...formData, firstName: e.target.value })} 
              />
            </div>
            <div className="form-group">
              <label>Last Name</label>
              <input 
                type="text" 
                required 
                value={formData.lastName} 
                onChange={e => setFormData({ ...formData, lastName: e.target.value })} 
              />
            </div>
            <div className="form-group">
              <label>Middle Name</label>
              <input 
                type="text" 
                value={formData.middleName} 
                onChange={e => setFormData({ ...formData, middleName: e.target.value })} 
              />
            </div>
            <div className="form-group">
              <label>Email</label>
              <input 
                type="email" 
                required 
                value={formData.email} 
                onChange={e => setFormData({ ...formData, email: e.target.value })} 
              />
            </div>
            {error && <p style={{ color: 'var(--danger)', marginBottom: '1rem' }}>{error}</p>}
            <div style={{ display: 'flex', gap: '1rem' }}>
              <button type="submit" className="primary">{editingEmployee ? 'Update' : 'Create'}</button>
              <button type="button" className="secondary" onClick={() => setIsFormOpen(false)}>Cancel</button>
            </div>
          </form>
        </div>
      )}

      <div className="grid">
        {employees.length === 0 && <p>No employees found.</p>}
        {employees.map(emp => (
          <div key={emp.id} className="card" style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            <div style={{ fontWeight: 'bold', fontSize: '1.1rem' }}>
              {emp.lastName} {emp.firstName} {emp.middleName}
            </div>
            <div style={{ color: 'var(--text-muted)' }}>{emp.email}</div>
            <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
              <button className="secondary" style={{ flex: 1 }} onClick={() => handleOpenEdit(emp)}>Edit</button>
              <button 
                className="secondary" 
                style={{ flex: 1, borderColor: 'var(--danger)', color: 'var(--danger)' }} 
                onClick={() => handleDelete(emp.id)}
              >
                Delete
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
