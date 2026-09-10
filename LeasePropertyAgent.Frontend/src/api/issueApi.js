import apiClient from "./client";

export async function processIssue(unitId, images) {
  const response = await apiClient.post("/issues/process", {
    unitId,
    images,
  });

  return response.data;
}