import { Slot } from "@radix-ui/react-slot";
import { cva, VariantProps } from "class-variance-authority";

const typographyVariants = cva(
    "",
    {
        variants: {
            variant: {
                default:
                    "leading-7 [&:not(:first-child)]:mt-6",
                h1:
                    "scroll-m-20 text-4xl font-extrabold tracking-tight lg:text-5xl",
                h2:
                    "mt-10 scroll-m-20 border-b pb-2 text-3xl font-semibold tracking-tight transition-colors first:mt-0",
                h3:
                    "scroll-m-20 text-2xl font-semibold tracking-tight",
                h4:
                    "scroll-m-20 text-xl font-semibold tracking-tight",
                blockquote:
                    "mt-6 border-l-2 pl-6 italic",
                inlinecode:
                    "relative rounded bg-muted px-[0.3rem] py-[0.2rem] font-mono text-sm font-semibold",
                lead:
                    "text-xl text-muted-foreground",
                large:
                    "text-lg font-semibold",
                small:
                    "text-sm font-medium leading-none",
                muted:
                    "text-sm text-muted-foreground",
            }
        },
        defaultVariants: {
            variant: "default"
        },
    }
)

const variantComps = {
    default: "p",
    h1: "h1",
    h2: "h2",
    h3: "h3",
    h4: "h4",
    blockquote: "blockquote",
    inlinecode: "code",
    lead: "p",
    large: "div",
    small: "small",
    muted: "p",
    null: "p",
    undefined: "p"
};

function Typography({
    className,
    variant,
    asChild = false,
    ...props
}: React.ComponentProps<"p"> &
    VariantProps<typeof typographyVariants> & {
        asChild?: boolean
    }) {
    const variantComp = variantComps[variant || "default"] || "p";
    const Comp = asChild ? Slot : variantComp;

    return (
        <Comp
            data-slot="typography"
            className={typographyVariants({ variant, className })}
            {...props}
        />
    );
}

export { Typography, typographyVariants };

