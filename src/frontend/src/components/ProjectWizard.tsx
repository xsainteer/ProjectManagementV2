import React, { useState } from 'react';
import { api } from '../api';
import { EmployeeSelect } from './EmployeeSelect';

interface Props {
  onComplete: () => void;
  onCancel: () => void;
}

export const ProjectWizard: React.FC<Props> = ({ onComplete, onCancel }) => {
  const [step, setStep] = useState(1);
  const [formData, setFormData] = useState({
    name: '',
    startDate: '',
    endDate: '',
    priority: 1,
    customerCompany: '',
    performerCompany: '',
    projectManagerId: 0,
    executorIds: [] as number[],
  });
  const [files, setFiles] = useState<File[]>([]);

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.dataTransfer.files) {
      setFiles([...files, ...Array.from(e.dataTransfer.files)]);
    }
  };

  const handleSubmit = async () => {
    const data = new FormData();
    data.append('Name', formData.name);
    data.append('StartDate', formData.startDate);
    data.append('EndDate', formData.endDate);
    data.append('Priority', formData.priority.toString());
    data.append('CustomerCompany', formData.customerCompany);
    data.append('PerformerCompany', formData.performerCompany);
    data.append('ProjectManagerId', formData.projectManagerId.toString());
    
    formData.executorIds.forEach(id => data.append('ExecutorIds', id.toString()));
    files.forEach(file => data.append('files', file));

    const res = await api.projects.createFull(data);
    if (res.isSuccess) {
      onComplete();
    } else {
      alert(res.error?.message || 'Error creating project');
    }
  };

  const renderStep = () => {
    switch(step) {
      case 1:
        return (
          <>
            <h3>Step 1: Basic Info</h3>
            <div className="form-group">
              <label>Project Name</label>
              <input type="text" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} />
            </div>
            <div className="form-group">
              <label>Start Date</label>
              <input type="date" value={formData.startDate} onChange={e => setFormData({...formData, startDate: e.target.value})} />
            </div>
            <div className="form-group">
              <label>End Date</label>
              <input type="date" value={formData.endDate} onChange={e => setFormData({...formData, endDate: e.target.value})} />
            </div>
            <div className="form-group">
              <label>Priority (1-10)</label>
              <input type="number" min="1" max="10" value={formData.priority} onChange={e => setFormData({...formData, priority: parseInt(e.target.value)})} />
            </div>
          </>
        );
      case 2:
        return (
          <>
            <h3>Step 2: Companies</h3>
            <div className="form-group">
              <label>Customer Company</label>
              <input type="text" value={formData.customerCompany} onChange={e => setFormData({...formData, customerCompany: e.target.value})} />
            </div>
            <div className="form-group">
              <label>Performer Company</label>
              <input type="text" value={formData.performerCompany} onChange={e => setFormData({...formData, performerCompany: e.target.value})} />
            </div>
          </>
        );
      case 3:
        return (
          <EmployeeSelect 
            label="Step 3: Select Project Manager" 
            value={formData.projectManagerId} 
            onChange={val => setFormData({...formData, projectManagerId: val})} 
          />
        );
      case 4:
        return (
          <EmployeeSelect 
            multiple 
            label="Step 4: Select Executors" 
            value={formData.executorIds} 
            onChange={val => setFormData({...formData, executorIds: val})} 
          />
        );
      case 5:
        return (
          <>
            <h3>Step 5: Upload Documents</h3>
            <div 
              className="dropzone"
              onDragOver={handleDrag}
              onDragEnter={handleDrag}
              onDrop={handleDrop}
            >
              Drag & Drop files here or click to select
              <input 
                type="file" 
                multiple 
                style={{display: 'none'}} 
                id="fileInput" 
                onChange={e => e.target.files && setFiles([...files, ...Array.from(e.target.files)])}
              />
              <label htmlFor="fileInput" style={{display: 'block', marginTop: '1rem', color: 'var(--primary)', cursor: 'pointer'}}>Browse Files</label>
            </div>
            <ul className="file-list">
              {files.map((f, i) => (
                <li key={i} className="file-item">
                  <span>{f.name}</span>
                  <button onClick={() => setFiles(files.filter((_, idx) => idx !== i))}>&times;</button>
                </li>
              ))}
            </ul>
          </>
        );
    }
  };

  return (
    <div className="card" style={{ maxWidth: '600px', margin: '0 auto' }}>
      <div className="wizard-steps">
        {[1,2,3,4,5].map(s => (
          <div key={s} className={`step-indicator ${step === s ? 'active' : step > s ? 'completed' : ''}`}>
            {s}
          </div>
        ))}
      </div>

      {renderStep()}

      <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '2rem' }}>
        <button className="secondary" onClick={step === 1 ? onCancel : () => setStep(step - 1)}>
          {step === 1 ? 'Cancel' : 'Back'}
        </button>
        {step < 5 ? (
          <button className="primary" onClick={() => setStep(step + 1)}>Next</button>
        ) : (
          <button className="primary" onClick={handleSubmit}>Create Project</button>
        )}
      </div>
    </div>
  );
};
