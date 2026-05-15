import { DataTableSkeleton } from "@/components/shared/skeletons";

export default function StrategiesLoading() {
  return <DataTableSkeleton columns={7} rows={5} hasPagination />;
}