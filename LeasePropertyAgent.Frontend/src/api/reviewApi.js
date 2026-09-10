import apiClient from "./client";

export async function reviewLeaseField(fieldId, request) {
  await apiClient.post(
    `/reviews/lease-fields/${fieldId}`,
    request
  );
}

export async function reviewLeaseFlag(flagId, request) {
  await apiClient.post(
    `/reviews/lease-flags/${flagId}`,
    request
  );
}

export async function reviewWorkOrder(workOrderId, request) {
  await apiClient.post(
    `/reviews/work-orders/${workOrderId}`,
    request
  );
}