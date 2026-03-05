import { useState, useEffect } from 'react';
import { ProjectList } from './components/ProjectList';
import { ProjectWizard } from './components/ProjectWizard';
import { EmployeeManager } from './components/EmployeeManager';
import { ProjectManager } from './components/ProjectManager';
import './index.css';

function App() {
  const [path, setPath] = useState(window.location.pathname);
  const [managedProjectId, setManagedProjectId] = useState<number | null>(null);

  useEffect(() => {
    const handlePopState = () => {
      setPath(window.location.pathname);
      // Try to extract project ID from URL if needed, but for now we'll use state
    };
    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, []);

  const navigate = (newPath: string) => {
    window.history.pushState({}, '', newPath);
    setPath(newPath);
    if (newPath !== '/manage') setManagedProjectId(null);
  };

  const handleManageProject = (id: number) => {
    setManagedProjectId(id);
    navigate('/manage');
  };

  return (
    <div className="app-container">
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1 style={{ cursor: 'pointer' }} onClick={() => navigate('/')}>Sibers Project Hub</h1>
        {(path === '/' || path === '/projects') && (
          <button className="primary" onClick={() => navigate('/wizard')}>+ New Project</button>
        )}
      </header>

      <nav className="navbar">
        <button 
          className={`nav-btn ${path === '/' || path === '/projects' || path === '/manage' ? 'active' : ''}`}
          onClick={() => navigate('/projects')}
        >
          Projects
        </button>
        <button 
          className={`nav-btn ${path === '/employees' ? 'active' : ''}`}
          onClick={() => navigate('/employees')}
        >
          Employees
        </button>
        <button 
          className={`nav-btn ${path === '/wizard' ? 'active' : ''}`}
          onClick={() => navigate('/wizard')}
        >
          Create Project
        </button>
      </nav>

      <main>
        {path === '/wizard' ? (
          <ProjectWizard 
            onComplete={() => navigate('/projects')}
            onCancel={() => navigate('/projects')}
          />
        ) : path === '/employees' ? (
          <EmployeeManager />
        ) : path === '/manage' && managedProjectId ? (
          <ProjectManager 
            projectId={managedProjectId} 
            onBack={() => navigate('/projects')} 
          />
        ) : (
          <ProjectList onManage={handleManageProject} />
        )}
      </main>
    </div>
  );
}

export default App;
