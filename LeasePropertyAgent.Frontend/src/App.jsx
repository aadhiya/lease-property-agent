import { useCallback, useEffect, useState } from "react";
import "./App.css";
import { getUnitWorkspace } from "./api/workspaceApi";
import apiClient from "./api/client";
import {
  reviewLeaseField,
  reviewLeaseFlag,
  reviewWorkOrder,
} from "./api/reviewApi";

function App() {
  const [units, setUnits] = useState([]);
  const [selectedUnitId, setSelectedUnitId] = useState("");
  const [workspace, setWorkspace] = useState(null);

  const [loadingUnits, setLoadingUnits] = useState(true);
  const [loadingWorkspace, setLoadingWorkspace] = useState(false);
  const [error, setError] = useState("");
const [reviewingId, setReviewingId] = useState("");

  const loadWorkspace = useCallback(async (unitId) => {
    try {
      setLoadingWorkspace(true);
      setError("");

      const data = await getUnitWorkspace(unitId);

      setWorkspace(data);
    } catch (err) {
      console.error(err);
      setWorkspace(null);
      setError("Unable to load the selected unit workspace.");
    } finally {
      setLoadingWorkspace(false);
    }
  }, []);
  const handleLeaseFieldReview = async (
    fieldId,
    action,
    reviewedValue = null
  ) => {
    try {
      setReviewingId(fieldId);
      setError("");

      await reviewLeaseField(fieldId, {
        action,
        reviewedValue,
      });

      await loadWorkspace(selectedUnitId);
    } catch (err) {
      console.error(err);
      setError("Unable to update the lease field review.");
    } finally {
      setReviewingId("");
    }
  };

  const handleLeaseFlagReview = async (
    flagId,
    action
  ) => {
    try {
      setReviewingId(flagId);
      setError("");

      await reviewLeaseFlag(flagId, {
        action,
      });

      await loadWorkspace(selectedUnitId);
    } catch (err) {
      console.error(err);
      setError("Unable to update the lease flag review.");
    } finally {
      setReviewingId("");
    }
  };

  const handleWorkOrderReview = async (
    workOrderId,
    action,
    title = null,
    description = null
  ) => {
    try {
      setReviewingId(workOrderId);
      setError("");

      await reviewWorkOrder(workOrderId, {
        action,
        title,
        description,
      });

      await loadWorkspace(selectedUnitId);
    } catch (err) {
      console.error(err);
      setError("Unable to update the work order review.");
    } finally {
      setReviewingId("");
    }
  };
  useEffect(() => {
    let cancelled = false;

    async function fetchUnits() {
  try {
    const response = await apiClient.get("/units");

    if (cancelled) {
      return;
    }

    setUnits(response.data);

    if (response.data.length > 0) {
      const firstUnitId = response.data[0].unitId;

      setSelectedUnitId(firstUnitId);
      await loadWorkspace(firstUnitId);
    }
  } catch (err) {
    if (cancelled) {
      return;
    }

    console.error(err);
    setError("Unable to load units.");
  } finally {
    if (!cancelled) {
      setLoadingUnits(false);
    }
  }
}

    fetchUnits();

    return () => {
      cancelled = true;
    };
}, [loadWorkspace]);


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

        <div className="unit-selector">
          <label htmlFor="unit-select">Select unit</label>

         <select
  id="unit-select"
  value={selectedUnitId}
  onChange={(event) => {
    const unitId = event.target.value;

    setSelectedUnitId(unitId);

    if (unitId) {
      loadWorkspace(unitId);
    }
  }}
  disabled={loadingUnits}
>
            {loadingUnits && <option>Loading units...</option>}

            {!loadingUnits && units.length === 0 && (
              <option value="">No units available</option>
            )}

            {units.map((unit) => (
              <option key={unit.unitId} value={unit.unitId}>
                {unit.unitNumber}
              </option>
            ))}
          </select>
        </div>
      </header>

      {error && (
        <div className="error-banner">
          {error}
        </div>
      )}

      <main className="workspace">
        {loadingWorkspace && (
          <div className="loading-state">
            Loading workspace...
          </div>
        )}

        {!loadingWorkspace && workspace && (
          <>
            <section className="unit-header">
              <div>
                <span className="label">Unit</span>
                <h2>{workspace.unitNumber}</h2>

                {workspace.property && workspace.building && (
                  <p className="location">
                    {workspace.property.name} · {workspace.building.name}
                  </p>
                )}
              </div>

              <div className="unit-meta">
                {workspace.unitType && (
                  <span>{workspace.unitType}</span>
                )}

                {workspace.area && (
                  <span>{workspace.area} sqm</span>
                )}

                <span className="status available">
                  {workspace.status}
                </span>
              </div>
            </section>

            <section className="workspace-grid">
              <LeasePanel
  lease={workspace.leases[0]}
  reviewingId={reviewingId}
  onReviewField={handleLeaseFieldReview}
  onReviewFlag={handleLeaseFlagReview}
/>

              <IssuesPanel
  issues={workspace.issues}
  unitNumber={workspace.unitNumber}
  reviewingId={reviewingId}
  onReviewWorkOrder={handleWorkOrderReview}
/>
            </section>
          </>
        )}
      </main>
    </div>
  );
}

function LeasePanel({
  lease,
  reviewingId,
  onReviewField,
  onReviewFlag,
}) {
  if (!lease) {
    return (
      <div className="card">
        <div className="card-header">
          <span className="label">Lease</span>
          <h3>Lease Overview</h3>
        </div>

        <div className="empty-state">
          No lease information is available for this unit.
        </div>
      </div>
    );
  }



  return (
    <div className="card">
      <div className="card-header">
        <div>
          <span className="label">Lease</span>
          <h3>Lease Overview</h3>
        </div>

        <span className="review-status">
          {lease.status}
        </span>
      </div>

      <div className="card-body">
        <div className="lease-summary">
          <div>
            <span className="field-label">Document</span>
            <strong>{lease.documentName}</strong>
          </div>

          <div>
            <span className="field-label">Monthly Rent</span>
            <strong>
              {lease.monthlyRent != null
                ? `${lease.currency ?? ""} ${lease.monthlyRent.toLocaleString()}`
                : "—"}
            </strong>
          </div>

          <div>
            <span className="field-label">Deposit</span>
            <strong>
              {lease.depositAmount != null
                ? `${lease.currency ?? ""} ${lease.depositAmount.toLocaleString()}`
                : "—"}
            </strong>
          </div>

          <div>
            <span className="field-label">Term</span>
            <strong>
              {lease.termMonths != null
                ? `${lease.termMonths} months`
                : "—"}
            </strong>
          </div>
        </div>

        <div className="section">
          <h4>Parties</h4>

          {lease.parties.map((party) => (
            <div className="party-row" key={party.id}>
              <div>
                <strong>{party.name || "Unknown party"}</strong>
                <span>{party.role}</span>
              </div>

              <span
                className={
                  party.isSigned
                    ? "signed"
                    : "not-signed"
                }
              >
                {party.isSigned ? "Signed" : "Not signed"}
              </span>
            </div>
          ))}
        </div>

        <div className="section">
  <h4>Extracted Fields</h4>

  {lease.fields.length === 0 ? (
    <p className="muted">No extracted fields.</p>
  ) : (
    <div className="field-list">
      {lease.fields.map((field) => {
        const isReviewing = reviewingId === field.id;

        return (
          <div className="field-row" key={field.id}>
            <div className="field-content">
              <strong>{field.fieldName}</strong>

              <span className="field-value">
                {field.reviewedValue ||
                  field.extractedValue ||
                  "—"}
              </span>

              <div className="field-evidence">
                {field.sourcePage && (
                  <span className="source">
                    Page {field.sourcePage}
                  </span>
                )}

                {field.sourceText && (
                  <span className="source-text">
                    "{field.sourceText}"
                  </span>
                )}
              </div>

              <div className="field-review-actions">
                <button
                  type="button"
                  disabled={isReviewing}
                  onClick={() =>
                    onReviewField(
                      field.id,
                      "accept"
                    )
                  }
                >
                  Accept
                </button>

                <button
                  type="button"
                  className="secondary-action"
                  disabled={isReviewing}
                  onClick={() =>
                    onReviewField(
                      field.id,
                      "reject"
                    )
                  }
                >
                  Reject
                </button>

                <button
                  type="button"
                  className="secondary-action"
                  disabled={isReviewing}
                  onClick={() => {
                    const currentValue =
                      field.reviewedValue ||
                      field.extractedValue ||
                      "";

                    const newValue =
                      window.prompt(
                        `Edit ${field.fieldName}`,
                        currentValue
                      );

                    if (
                      newValue !== null &&
                      newValue.trim() !== ""
                    ) {
                      onReviewField(
                        field.id,
                        "edit",
                        newValue.trim()
                      );
                    }
                  }}
                >
                  Edit
                </button>
              </div>
            </div>

            <div className="field-meta">
              <span className="confidence">
                {field.confidence != null
                  ? `${Math.round(
                      field.confidence * 100
                    )}%`
                  : "—"}
              </span>

              <span className="review-status">
                {field.reviewStatus}
              </span>
            </div>
          </div>
        );
      })}
    </div>
  )}
</div>

        <div className="section">
          <h4>Validation</h4>

          {lease.validationResults.length === 0 ? (
            <p className="muted">
              No validation results.
            </p>
          ) : (
            <div className="validation-list">
              {lease.validationResults.map((result) => (
                <div
                  className="validation-row"
                  key={result.id}
                >
                  <div>
                    <strong>{result.ruleId}</strong>
                    <span>{result.reason}</span>
                  </div>

                  <span
                    className={`validation-status ${result.status.toLowerCase()}`}
                  >
                    {result.status}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="section">
  <h4>Flags</h4>

  {lease.flags.length === 0 ? (
    <p className="muted">
      No flags detected.
    </p>
  ) : (
    <div className="flag-list">
      {lease.flags.map((flag) => {
        const isReviewing =
          reviewingId === flag.id;

        return (
          <div
            className="flag-row"
            key={flag.id}
          >
            <div className="flag-content">
              <strong>{flag.type}</strong>

              <span>{flag.message}</span>

              {flag.sourcePage && (
                <small>
                  Page {flag.sourcePage}
                </small>
              )}

              {flag.sourceText && (
                <small className="source-text">
                  "{flag.sourceText}"
                </small>
              )}

              <div className="field-review-actions">
                <button
                  type="button"
                  disabled={isReviewing}
                  onClick={() =>
                    onReviewFlag(
                      flag.id,
                      "accept"
                    )
                  }
                >
                  Accept
                </button>

                <button
                  type="button"
                  className="secondary-action"
                  disabled={isReviewing}
                  onClick={() =>
                    onReviewFlag(
                      flag.id,
                      "reject"
                    )
                  }
                >
                  Reject
                </button>

                <button
                  type="button"
                  className="secondary-action"
                  disabled={isReviewing}
                  onClick={() =>
                    onReviewFlag(
                      flag.id,
                      "dismiss"
                    )
                  }
                >
                  Dismiss
                </button>
              </div>
            </div>

            <div className="flag-meta">
              <span
                className={`severity ${flag.severity.toLowerCase()}`}
              >
                {flag.severity}
              </span>

              <span className="review-status">
                {flag.status}
              </span>
            </div>
          </div>
        );
      })}
    </div>
  )}
</div>
      </div>
    </div>
  );
}

function IssuesPanel({
  issues,
  unitNumber,
  reviewingId,
  onReviewWorkOrder,
}) {
  if (!issues || issues.length === 0) {
    return (
      <div className="card">
        <div className="card-header">
          <span className="label">Issues</span>
          <h3>Property Issues</h3>
        </div>

        <div className="empty-state">
          No property issues have been reported for this unit.
        </div>
      </div>
    );
  }

  return (
    <div className="card">
      <div className="card-header">
        <div>
          <span className="label">Issues</span>
          <h3>Property Issues</h3>
        </div>

        <span className="count-badge">
          {issues.length}
        </span>
      </div>

      <div className="card-body">
        {issues.map((issue) => (
          <div className="issue" key={issue.id}>
            <div className="issue-heading">
              <div>
                <h4>{issue.title}</h4>
                <p>{issue.description}</p>
              </div>

              <span className="review-status">
                {issue.status}
              </span>
            </div>

            {issue.conditionAssessment && (
              <div className="assessment">
                <span className="field-label">
                  AI assessment
                </span>

                <p>{issue.conditionAssessment}</p>

                {issue.confidence != null && (
                  <span className="confidence">
                    {Math.round(issue.confidence * 100)}% confidence
                  </span>
                )}
              </div>
            )}

            {issue.images.length > 0 && (
              <div className="section">
                <h4>Image Evidence</h4>

                {issue.images.map((image) => (
                  <div
                    className="image-evidence"
                    key={image.id}
                  >
                    <strong>{image.fileName}</strong>

                    <span>
                      {image.observation ||
                        "No observation available."}
                    </span>

                    {image.confidence != null && (
                      <small>
                        {Math.round(
                          image.confidence * 100
                        )}
                        % confidence
                      </small>
                    )}
                  </div>
                ))}
              </div>
            )}

            {issue.workOrders.length > 0 && (
              <div className="section">
                <h4>Work Orders</h4>

                {issue.workOrders?.map((workOrder) => {
  const isReviewing =
    reviewingId === workOrder.id;

  return (
    <div
      className="work-order-card"
      key={workOrder.id}
    >
      <div className="work-order-header">
        <div>
          <div className="work-order-label">
            DRAFT WORK ORDER
          </div>

          <h4>{workOrder.title}</h4>
        </div>

        <span className="review-status">
          {workOrder.status}
        </span>
      </div>

      <p>{workOrder.description}</p>

      <div className="work-order-unit">
        Affected unit:{" "}
        <strong>{unitNumber}</strong>
      </div>

      <div className="review-actions">
        <button
          type="button"
          disabled={isReviewing}
          onClick={() =>
            onReviewWorkOrder(
              workOrder.id,
              "accept"
            )
          }
        >
          Accept
        </button>

        <button
          type="button"
          className="secondary-action"
          disabled={isReviewing}
          onClick={() =>
            onReviewWorkOrder(
              workOrder.id,
              "reject"
            )
          }
        >
          Reject
        </button>

        <button
          type="button"
          className="secondary-action"
          disabled={isReviewing}
          onClick={() => {
            const newTitle =
              window.prompt(
                "Work order title",
                workOrder.title
              );

            if (newTitle === null) {
              return;
            }

            const newDescription =
              window.prompt(
                "Work order description",
                workOrder.description
              );

            if (newDescription === null) {
              return;
            }

            if (
              newTitle.trim() &&
              newDescription.trim()
            ) {
              onReviewWorkOrder(
                workOrder.id,
                "edit",
                newTitle.trim(),
                newDescription.trim()
              );
            }
          }}
        >
          Edit
        </button>
      </div>
    </div>
  );
})}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  );
}

export default App;