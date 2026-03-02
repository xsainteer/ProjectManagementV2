import { useState, useEffect } from 'react';
import { ProjectList } from './components/ProjectList';
import { ProjectWizard } from './components/ProjectWizard';
import { EmployeeManager } from './components/EmployeeManager';
import './index.css';

function App() {
  const [path, setPath] = useState(window.location.pathname);

  useEffect(() => {
    const handlePopState = () => setPath(window.location.pathname);
    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, []);

  const navigate = (newPath: string) => {
    window.history.pushState({}, '', newPath);
    setPath(newPath);
  };

  return (
    <div className="app-container">
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1 style={{ cursor: 'pointer' }} onClick={() => navigate('/')}>Sibers Project Hub</h1>
        {path === '/projects' && (
          <button className="primary" onClick={() => navigate('/wizard')}>+ New Project</button>
        )}
      </header>

      <nav className="navbar">
        <button 
          className={`nav-btn ${path === '/' || path === '/projects' ? 'active' : ''}`}
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
        ) : (
          <ProjectList />
        )}
      </main>
    </div>
  );
}

export default App;
