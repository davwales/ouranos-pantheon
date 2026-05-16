import { DataTableSkeleton } from "@/components/shared/skeletons";

export default function SignalsLoading() {
  return <DataTableSkeleton columns={12} rows={5} hasPagination />;
}