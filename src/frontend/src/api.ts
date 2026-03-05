import type {Project, Employee, Result, ProjectDetails, ProjectDocument} from './types';

const API_BASE = '/api';

const handleResponse = async <T>(response: Response): Promise<Result<T>> => {
  if (response.ok) {
    if (response.status === 204) {
      return { isSuccess: true, value: undefined as any };
    }
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
    getDetails: (id: number): Promise<Result<ProjectDetails>> => 
      fetch(`${API_BASE}/projects/${id}/details`).then(handleResponse),
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
    update: (id: number, data: any): Promise<Result<void>> => 
      fetch(`${API_BASE}/projects/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
      }).then(handleResponse),
    delete: (id: number): Promise<Result<void>> => 
      fetch(`${API_BASE}/projects/${id}`, { method: 'DELETE' }).then(r => ({ isSuccess: r.ok, value: undefined as any })),
    addEmployee: (projectId: number, employeeId: number): Promise<Result<void>> => 
      fetch(`${API_BASE}/projects/${projectId}/employees/${employeeId}`, { method: 'POST' }).then(handleResponse),
    removeEmployee: (projectId: number, employeeId: number): Promise<Result<void>> => 
      fetch(`${API_BASE}/projects/${projectId}/employees/${employeeId}`, { method: 'DELETE' }).then(handleResponse),
  },
  documents: {
    upload: (projectId: number, file: File): Promise<Result<ProjectDocument>> => {
      const formData = new FormData();
      formData.append('projectId', projectId.toString());
      formData.append('file', file);
      return fetch(`${API_BASE}/documents`, {
        method: 'POST',
        body: formData
      }).then(handleResponse);
    },
    delete: (id: number): Promise<Result<void>> => 
      fetch(`${API_BASE}/documents/${id}`, { method: 'DELETE' }).then(r => ({ isSuccess: r.ok, value: undefined as any })),
    download: (id: number, fileName: string) => {
      window.open(`${API_BASE}/documents/${id}/download`, '_blank');
    }
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
