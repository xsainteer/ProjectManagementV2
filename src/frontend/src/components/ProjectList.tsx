import React, { useState, useEffect } from 'react';
import { api } from '../api';
import type {Project} from '../types';

export const ProjectList: React.FC = () => {
  const [projects, setProjects] = useState<Project[]>([]);
  const [filters, setFormData] = useState({
    startDateFrom: '',
    startDateTo: '',
    priority: '',
    sortBy: 'Id',
    sortDescending: false
  });

  const fetchProjects = () => {
    api.projects.getAll(filters).then(res => {
      if (res.isSuccess) {
        setProjects(Array.isArray(res.value) ? res.value : (res.value as any).items);
      }
    });
  };

  useEffect(fetchProjects, [filters]);

  const handleDelete = async (id: number) => {
    if (confirm('Are you sure?')) {
      await api.projects.delete(id);
      fetchProjects();
    }
  };

  return (
    <div>
      <div className="card" style={{ marginBottom: '2rem', display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
        <div className="form-group">
          <label>From Date</label>
          <input type="date" onChange={e => setFormData({...filters, startDateFrom: e.target.value})} />
        </div>
        <div className="form-group">
          <label>To Date</label>
          <input type="date" onChange={e => setFormData({...filters, startDateTo: e.target.value})} />
        </div>
        <div className="form-group">
          <label>Priority</label>
          <input type="number" placeholder="Filter by priority" onChange={e => setFormData({...filters, priority: e.target.value})} />
        </div>
        <div className="form-group">
          <label>Sort By</label>
          <select onChange={e => setFormData({...filters, sortBy: e.target.value})}>
            <option value="Id">ID</option>
            <option value="Name">Name</option>
            <option value="Priority">Priority</option>
            <option value="StartDate">Start Date</option>
          </select>
        </div>
      </div>

      <div className="grid">
        {projects.map(p => (
          <div key={p.id} className="card">
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
              <span className="priority">Priority {p.priority}</span>
              <button style={{ color: 'var(--danger)', background: 'none', border: 'none', cursor: 'pointer' }} onClick={() => handleDelete(p.id)}>Delete</button>
            </div>
            <h3 style={{ margin: '0.5rem 0' }}>{p.name}</h3>
            <p style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
              {p.customerCompany} &rarr; {p.performerCompany}
            </p>
            <small>Started: {new Date(p.startDate).toLocaleDateString()}</small>
          </div>
        ))}
      </div>
    </div>
  );
};
