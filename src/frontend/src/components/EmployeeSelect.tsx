import React, { useState, useEffect } from 'react';
import { api } from '../api';
import type {Employee} from '../types';

interface Props {
  label: string;
  multiple?: boolean;
  value: number | number[];
  onChange: (val: any) => void;
}

export const EmployeeSelect: React.FC<Props> = ({ label, multiple, value, onChange }) => {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [search, setSearch] = useState('');
  const [loading, setLoading] = useState(false);
  const [allSelected, setAllSelected] = useState<Employee[]>([]);

  // Keep track of all selected employees to show their names even if they're filtered out
  useEffect(() => {
    const selectedIds = Array.isArray(value) ? value : (value === 0 ? [] : [value]);
    if (selectedIds.length === 0) {
      setAllSelected([]);
      return;
    }

    // Only fetch if we don't have them yet or if the value changed
    const missingIds = selectedIds.filter(id => !allSelected.find(e => e.id === id));
    if (missingIds.length > 0) {
      // In a real app, we'd have a getByIds endpoint. Here we'll just fetch all and filter
      api.employees.getAll().then(res => {
        if (res.isSuccess) {
          const found = res.value.filter(e => selectedIds.includes(e.id));
          setAllSelected(found);
        }
      });
    } else {
      setAllSelected(allSelected.filter(e => selectedIds.includes(e.id)));
    }
  }, [value]);

  useEffect(() => {
    const delayDebounceFn = setTimeout(() => {
      const fetchParams = search ? { search } : {};
      setLoading(true);
      api.employees.getAll(fetchParams).then(res => {
        if (res.isSuccess) {
          const data = res.value;
          setEmployees(Array.isArray(data) ? data : (data as any).items || []);
        }
        setLoading(false);
      });
    }, 300);

    return () => clearTimeout(delayDebounceFn);
  }, [search]);

  const handleToggle = (id: number) => {
    if (multiple) {
      const current = Array.isArray(value) ? value : [];
      if (current.includes(id)) {
        onChange(current.filter(i => i !== id));
      } else {
        onChange([...current, id]);
      }
    } else {
      onChange(id);
    }
  };

  return (
    <div className="form-group">
      <label>{label}</label>
      
      {/* Selection Pills */}
      {multiple && allSelected.length > 0 && (
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem', marginBottom: '0.5rem' }}>
          {allSelected.map(e => (
            <span key={e.id} style={{ 
              background: 'var(--primary)', 
              color: 'white', 
              padding: '0.25rem 0.5rem', 
              borderRadius: '1rem', 
              fontSize: '0.8rem',
              display: 'flex',
              alignItems: 'center',
              gap: '0.25rem'
            }}>
              {e.firstName} {e.lastName}
              <button 
                onClick={() => handleToggle(e.id)}
                style={{ background: 'none', border: 'none', color: 'white', cursor: 'pointer', padding: 0, fontWeight: 'bold' }}
              >
                &times;
              </button>
            </span>
          ))}
        </div>
      )}

      <input 
        type="text" 
        placeholder="Type to search employees..." 
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        style={{ marginBottom: '0.5rem' }}
      />

      <div style={{ 
        border: '1px solid var(--border)', 
        borderRadius: '0.375rem', 
        maxHeight: '200px', 
        overflowY: 'auto',
        background: 'white'
      }}>
        {employees.length === 0 && !loading && (
          <div style={{ padding: '0.5rem', color: 'var(--text-muted)' }}>No employees found</div>
        )}
        {employees.map(e => {
          const isSelected = multiple 
            ? (Array.isArray(value) && value.includes(e.id))
            : value === e.id;
          
          return (
            <div 
              key={e.id}
              onClick={() => handleToggle(e.id)}
              style={{ 
                padding: '0.5rem', 
                cursor: 'pointer',
                background: isSelected ? '#eff6ff' : 'transparent',
                borderBottom: '1px solid var(--border)',
                display: 'flex',
                alignItems: 'center',
                gap: '0.5rem'
              }}
            >
              <input 
                type={multiple ? "checkbox" : "radio"} 
                checked={isSelected} 
                readOnly 
              />
              <div>
                <div style={{ fontWeight: isSelected ? '600' : '400' }}>{e.lastName} {e.firstName}</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{e.email}</div>
              </div>
            </div>
          );
        })}
        {loading && <div style={{ padding: '0.5rem' }}>Searching...</div>}
      </div>
    </div>
  );
};
