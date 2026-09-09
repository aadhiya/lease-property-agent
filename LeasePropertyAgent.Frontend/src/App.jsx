import "./App.css";

function App() {
  return (
    <div className="app">
      <header className="app-header">
        <div>
          <p className="eyebrow">TrueLinks AI</p>
          <h1>Property Workspace</h1>
          <p className="subtitle">
            Review leases, validation results, and property issues in one place.
          </p>
        </div>
      </header>

      <main className="workspace">
        <section className="unit-header">
          <div>
            <span className="label">Unit</span>
            <h2>MC-B-1204</h2>
          </div>

          <div className="unit-meta">
            <span>Apartment</span>
            <span>118 sqm</span>
            <span className="status available">Available</span>
          </div>
        </section>

        <section className="workspace-grid">
          <div className="card">
            <div className="card-header">
              <div>
                <span className="label">Lease</span>
                <h3>Lease Overview</h3>
              </div>
            </div>

            <div className="empty-state">
              <p>Lease information will appear here.</p>
            </div>
          </div>

          <div className="card">
            <div className="card-header">
              <div>
                <span className="label">Issues</span>
                <h3>Property Issues</h3>
              </div>
            </div>

            <div className="empty-state">
              <p>Property issues will appear here.</p>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}

export default App;