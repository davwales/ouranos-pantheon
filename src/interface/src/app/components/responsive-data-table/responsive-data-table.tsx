import { DesktopContent, MobileContent, ResponsiveContent } from "@/app/components/responsive-content";
import DesktopDataTable from "@/app/components/responsive-data-table/desktop-data-table";
import MobileDataTable from "@/app/components/responsive-data-table/mobile-data-table";
import { DataTableProps } from "@/app/components/responsive-data-table/types";
import React from "react";

export default function ResponsiveDataTable<TData>({
    ...props
}: React.ComponentProps<"div"> & DataTableProps<TData>) {
    return (
        <ResponsiveContent>
            <DesktopContent>
                <DesktopDataTable {...props} />
            </DesktopContent>

            <MobileContent>
                <MobileDataTable {...props} />
            </MobileContent>
        </ResponsiveContent>
    );
}