import apiClient from "./client";

export async function getUnitWorkspace(unitId) {
  const response = await apiClient.get(
    `/units/${unitId}/workspace`
  );

  return response.data;
}