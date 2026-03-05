import React, { useState, useEffect, useCallback } from 'react';
import { api } from '../api';
import type { ProjectDetails, Employee } from '../types';
import { EmployeeSelect } from './EmployeeSelect';

interface Props {
  projectId: number;
  onBack: () => void;
}

export const ProjectManager: React.FC<Props> = ({ projectId, onBack }) => {
  const [project, setProject] = useState<ProjectDetails | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<'info' | 'employees' | 'documents'>('info');
  const [isAddingEmployee, setIsAddingEmployee] = useState(false);
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<number>(0);
  
  // Edit mode state
  const [isEditingInfo, setIsEditingInfo] = useState(false);
  const [editFormData, setEditFormData] = useState({
    name: '',
    customerCompany: '',
    performerCompany: '',
    projectManagerId: 0,
    startDate: '',
    endDate: '',
    priority: 1
  });

  const fetchProject = useCallback(async () => {
    setLoading(true);
    const res = await api.projects.getDetails(projectId);
    if (res.isSuccess) {
      setProject(res.value);
      // Initialize edit form
      setEditFormData({
        name: res.value.name,
        customerCompany: res.value.customerCompany,
        performerCompany: res.value.performerCompany,
        projectManagerId: res.value.projectManagerId,
        startDate: res.value.startDate.split('T')[0],
        endDate: res.value.endDate?.split('T')[0] || '',
        priority: res.value.priority
      });
    } else {
      setError(res.error?.message || 'Failed to load project');
    }
    setLoading(false);
  }, [projectId]);

  useEffect(() => {
    fetchProject();
  }, [fetchProject]);

  const handleUpdateProject = async (e: React.FormEvent) => {
    e.preventDefault();
    const res = await api.projects.update(projectId, {
      id: projectId,
      ...editFormData,
      endDate: editFormData.endDate || null
    });
    
    if (res.isSuccess) {
      setIsEditingInfo(false);
      fetchProject();
    } else {
      alert(res.error?.message || 'Error updating project');
    }
  };

  const handleAddEmployee = async () => {
    if (selectedEmployeeId === 0) return;
    const res = await api.projects.addEmployee(projectId, selectedEmployeeId);
    if (res.isSuccess) {
      setIsAddingEmployee(false);
      setSelectedEmployeeId(0);
      fetchProject();
    } else {
      alert(res.error?.message || 'Error adding employee');
    }
  };

  const handleRemoveEmployee = async (employeeId: number) => {
    if (confirm('Remove this employee from the project?')) {
      const res = await api.projects.removeEmployee(projectId, employeeId);
      if (res.isSuccess) {
        fetchProject();
      } else {
        alert(res.error?.message || 'Error removing employee');
      }
    }
  };

  const handleUploadFile = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const res = await api.documents.upload(projectId, e.target.files[0]);
      if (res.isSuccess) {
        fetchProject();
      } else {
        alert(res.error?.message || 'Error uploading file');
      }
    }
  };

  const handleDeleteDocument = async (docId: number) => {
    if (confirm('Delete this document?')) {
      const res = await api.documents.delete(docId);
      if (res.isSuccess) {
        fetchProject();
      } else {
        alert(res.error?.message || 'Error deleting document');
      }
    }
  };

  if (loading) return <div>Loading project...</div>;
  if (error) return <div className="card" style={{ color: 'var(--danger)' }}>{error} <button onClick={onBack}>Go Back</button></div>;
  if (!project) return null;

  return (
    <div className="project-manager">
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '2rem' }}>
        <button className="secondary" onClick={onBack}>&larr; Back</button>
        <h2 style={{ margin: 0 }}>Manage Project: {project.name}</h2>
      </div>

      <div className="navbar" style={{ marginBottom: '1.5rem' }}>
        <button 
          className={`nav-btn ${activeTab === 'info' ? 'active' : ''}`}
          onClick={() => setActiveTab('info')}
        >
          Project Info
        </button>
        <button 
          className={`nav-btn ${activeTab === 'employees' ? 'active' : ''}`}
          onClick={() => setActiveTab('employees')}
        >
          Employees ({project.employees.length})
        </button>
        <button 
          className={`nav-btn ${activeTab === 'documents' ? 'active' : ''}`}
          onClick={() => setActiveTab('documents')}
        >
          Documents ({project.documents.length})
        </button>
      </div>

      <div className="card">
        {activeTab === 'info' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h3 style={{ margin: 0 }}>Project Information</h3>
              {!isEditingInfo && (
                <button className="secondary" onClick={() => setIsEditingInfo(true)}>Edit Info</button>
              )}
            </div>

            {isEditingInfo ? (
              <form onSubmit={handleUpdateProject} style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
                <div className="form-group" style={{ gridColumn: 'span 2' }}>
                  <label>Project Name</label>
                  <input type="text" value={editFormData.name} onChange={e => setEditFormData({...editFormData, name: e.target.value})} required />
                </div>
                <div className="form-group">
                  <label>Customer Company</label>
                  <input type="text" value={editFormData.customerCompany} onChange={e => setEditFormData({...editFormData, customerCompany: e.target.value})} required />
                </div>
                <div className="form-group">
                  <label>Performer Company</label>
                  <input type="text" value={editFormData.performerCompany} onChange={e => setEditFormData({...editFormData, performerCompany: e.target.value})} required />
                </div>
                <div className="form-group" style={{ gridColumn: 'span 2' }}>
                  <EmployeeSelect 
                    label="Project Manager" 
                    value={editFormData.projectManagerId} 
                    onChange={val => setEditFormData({...editFormData, projectManagerId: val})} 
                  />
                </div>
                <div className="form-group">
                  <label>Start Date</label>
                  <input type="date" value={editFormData.startDate} onChange={e => setEditFormData({...editFormData, startDate: e.target.value})} required />
                </div>
                <div className="form-group">
                  <label>End Date</label>
                  <input type="date" value={editFormData.endDate} onChange={e => setEditFormData({...editFormData, endDate: e.target.value})} />
                </div>
                <div className="form-group">
                  <label>Priority</label>
                  <input type="number" min="1" max="10" value={editFormData.priority} onChange={e => setEditFormData({...editFormData, priority: parseInt(e.target.value)})} required />
                </div>
                <div style={{ gridColumn: 'span 2', display: 'flex', gap: '1rem', marginTop: '1rem' }}>
                  <button type="submit" className="primary">Save Changes</button>
                  <button type="button" className="secondary" onClick={() => setIsEditingInfo(false)}>Cancel</button>
                </div>
              </form>
            ) : (
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '2rem' }}>
                <div>
                  <p><strong>Customer:</strong> {project.customerCompany}</p>
                  <p><strong>Performer:</strong> {project.performerCompany}</p>
                  <p><strong>Manager:</strong> {project.projectManager.firstName} {project.projectManager.lastName} ({project.projectManager.email})</p>
                  <p><strong>Priority:</strong> <span className="priority">Level {project.priority}</span></p>
                </div>
                <div>
                  <p><strong>Start Date:</strong> {new Date(project.startDate).toLocaleDateString()}</p>
                  <p><strong>End Date:</strong> {project.endDate ? new Date(project.endDate).toLocaleDateString() : 'Not set'}</p>
                </div>
              </div>
            )}
          </div>
        )}

        {activeTab === 'employees' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <h3 style={{ margin: 0 }}>Assigned Employees</h3>
              <button className="primary" onClick={() => setIsAddingEmployee(true)}>+ Assign Employee</button>
            </div>

            {isAddingEmployee && (
              <div className="card" style={{ marginBottom: '1.5rem', background: 'var(--bg)' }}>
                <EmployeeSelect 
                  label="Select employee to add" 
                  value={selectedEmployeeId} 
                  onChange={setSelectedEmployeeId} 
                />
                <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem' }}>
                  <button className="primary" onClick={handleAddEmployee} disabled={selectedEmployeeId === 0}>Add to Project</button>
                  <button className="secondary" onClick={() => setIsAddingEmployee(false)}>Cancel</button>
                </div>
              </div>
            )}

            <div className="grid" style={{ gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))' }}>
              {project.employees.map(emp => (
                <div key={emp.id} className="card" style={{ padding: '1rem', background: 'var(--bg)', border: 'none' }}>
                  <div style={{ fontWeight: '600' }}>{emp.firstName} {emp.lastName}</div>
                  <div style={{ fontSize: '0.875rem', color: 'var(--text-muted)' }}>{emp.email}</div>
                  <button 
                    className="secondary" 
                    style={{ marginTop: '0.75rem', width: '100%', color: 'var(--danger)', borderColor: 'var(--danger)' }}
                    onClick={() => handleRemoveEmployee(emp.id)}
                  >
                    Remove
                  </button>
                </div>
              ))}
              {project.employees.length === 0 && <p>No employees assigned yet.</p>}
            </div>
          </div>
        )}

        {activeTab === 'documents' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
              <h3 style={{ margin: 0 }}>Project Documents</h3>
              <label className="primary" style={{ cursor: 'pointer', display: 'inline-block', padding: '0.625rem 1.25rem', borderRadius: '0.375rem', fontWeight: 600 }}>
                + Upload Document
                <input type="file" style={{ display: 'none' }} onChange={handleUploadFile} />
              </label>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
              {project.documents.map(doc => (
                <div key={doc.id} className="file-item" style={{ padding: '1rem', border: '1px solid var(--border)', background: 'var(--bg)' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', flex: 1 }}>
                    <div style={{ fontWeight: '500' }}>{doc.fileName}</div>
                  </div>
                  <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button className="secondary" onClick={() => api.documents.download(doc.id, doc.fileName)}>Download</button>
                    <button className="secondary" style={{ color: 'var(--danger)', borderColor: 'var(--danger)' }} onClick={() => handleDeleteDocument(doc.id)}>Delete</button>
                  </div>
                </div>
              ))}
              {project.documents.length === 0 && <p>No documents uploaded yet.</p>}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
