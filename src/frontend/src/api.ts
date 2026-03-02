import type {Project, Employee, Result} from './types';

const API_BASE = '/api';

const handleResponse = async <T>(response: Response): Promise<Result<T>> => {
  if (response.ok) {
    const value = await response.json();
    return { isSuccess: true, value };
  } else {
    try {
      const error = await response.json();
      return { 
        isSuccess: false, 
        value: null as any, 
        error: { 
          message: error.detail || error.title || 'An error occurred', 
          type: response.status 
        } 
      };
    } catch {
      return { 
        isSuccess: false, 
        value: null as any, 
        error: { message: 'Network error', type: 500 } 
      };
    }
  }
};

export const api = {
  projects: {
    getAll: (params: any): Promise<Result<any>> => {
      const query = new URLSearchParams(params).toString();
      return fetch(`${API_BASE}/projects?${query}`).then(handleResponse);
    },
    getById: (id: number): Promise<Result<Project>> => 
      fetch(`${API_BASE}/projects/${id}`).then(handleResponse),
    create: (data: any): Promise<Result<Project>> => 
      fetch(`${API_BASE}/projects`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      }).then(handleResponse),
    createFull: (formData: FormData): Promise<Result<Project>> => 
      fetch(`${API_BASE}/projects/full`, {
        method: 'POST',
        body: formData
      }).then(handleResponse),
    delete: (id: number): Promise<Result<void>> => 
      fetch(`${API_BASE}/projects/${id}`, { method: 'DELETE' }).then(r => ({ isSuccess: r.ok, value: undefined as any }))
  },
  employees: {
    getAll: (params?: any): Promise<Result<Employee[]>> => {
      const query = params ? `?${new URLSearchParams(params).toString()}` : '';
      return fetch(`${API_BASE}/employees${query}`).then(handleResponse);
    },
    getById: (id: number): Promise<Result<Employee>> => 
      fetch(`${API_BASE}/employees/${id}`).then(handleResponse),
    create: (data: any): Promise<Result<Employee>> => 
      fetch(`${API_BASE}/employees`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      }).then(handleResponse),
    update: (id: number, data: any): Promise<Result<void>> => 
      fetch(`${API_BASE}/employees/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      }).then(r => ({ isSuccess: r.ok, value: undefined as any })),
    delete: (id: number): Promise<Result<void>> => 
      fetch(`${API_BASE}/employees/${id}`, { method: 'DELETE' }).then(r => ({ isSuccess: r.ok, value: undefined as any }))
  }
};
