import { useState } from 'react';
import { ProjectList } from './components/ProjectList';
import { ProjectWizard } from './components/ProjectWizard';
import './index.css';

function App() {
  const [view, setView] = useState<'list' | 'wizard'>('wizard');

  return (
    <div className="app-container">
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Sibers Project Hub</h1>
        {view === 'list' && (
          <button className="primary" onClick={() => setView('wizard')}>+ New Project</button>
        )}
      </header>

      <nav className="navbar">
        <button 
          className={`nav-btn ${view === 'list' ? 'active' : ''}`}
          onClick={() => setView('list')}
        >
          Projects
        </button>
      </nav>

      <main>
        {view === 'list' ? (
          <ProjectList />
        ) : (
          <ProjectWizard 
            onComplete={() => setView('list')}
            onCancel={() => setView('list')}
          />
        )}
      </main>
    </div>
  );
}

export default App;
