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

  return (
    <div className="form-group">
      <label>{label}</label>
      <input 
        type="text" 
        placeholder="Type to search..." 
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        style={{ marginBottom: '0.5rem' }}
      />
      <select 
        multiple={multiple}
        value={value as any}
        onChange={(e) => {
          if (multiple) {
            const options = Array.from(e.target.selectedOptions, o => parseInt(o.value));
            onChange(options);
          } else {
            onChange(parseInt(e.target.value));
          }
        }}
      >
        <option value="0">-- Select Employee --</option>
        {employees.map(e => (
          <option key={e.id} value={e.id}>
            {e.lastName} {e.firstName} ({e.email})
          </option>
        ))}
      </select>
      {loading && <small>Searching...</small>}
    </div>
  );
};
