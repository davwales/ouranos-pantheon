import {
  Content,
  ResponsiveContent,
} from "@/app/components/responsive-content";
import DesktopDataTable from "@/app/components/responsive-data-table/desktop-data-table";
import MobileDataTable from "@/app/components/responsive-data-table/mobile-data-table";
import { DataTableProps } from "@/app/components/responsive-data-table/types";
import {
  DesktopDataTableSkeleton,
  MobileDataTableSkeleton,
} from "@/app/components/skeletons/data-table-skeleton";
import React from "react";

export default function ResponsiveDataTable<TData>({
  loading = false,
  skeletonRows = 3,
  ...props
}: React.ComponentProps<"div"> & DataTableProps<TData>) {
  if (loading) {
    const skeletonProps = {
      columns: props.columns.length || 5,
      rows: props.data?.length || 5,
      hasFilters: !props.disableFiltering,
      hasPagination:
        !props.disablePagination && !!props.state?.pagination,
      className: props.className,
    };

    return (
      <ResponsiveContent>
        <Content type="desktop">
          <DesktopDataTableSkeleton {...skeletonProps} />
        </Content>
        <Content type="mobile">
          <MobileDataTableSkeleton
            rows={skeletonRows}
            hasFilters={skeletonProps.hasFilters}
            hasPagination={skeletonProps.hasPagination}
            className={skeletonProps.className}
          />
        </Content>
      </ResponsiveContent>
    );
  }

  return (
    <ResponsiveContent>
      <Content type="desktop">
        <DesktopDataTable {...props} />
      </Content>
      <Content type="mobile">
        <MobileDataTable {...props} />
      </Content>
    </ResponsiveContent>
  );
}
