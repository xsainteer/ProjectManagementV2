export interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  middleName?: string;
  email: string;
}

export interface Project {
  id: number;
  name: string;
  customerCompany: string;
  performerCompany: string;
  projectManagerId: number;
  startDate: string;
  endDate?: string;
  priority: number;
}

export interface ProjectDocument {
  id: number;
  fileName: string;
  filePath: string;
  projectId: number;
}

export interface ProjectDetails extends Project {
  projectManager: Employee;
  employees: Employee[];
  documents: ProjectDocument[];
}

export interface ProjectTask {
  id: number;
  name: string;
  authorId: number;
  executorId: number;
  status: 'ToDo' | 'InProgress' | 'Done';
  comment?: string;
  priority: number;
  projectId: number;
}

export interface Result<T> {
  isSuccess: boolean;
  value: T;
  error?: {
    message: string;
    type: number;
  };
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
}
