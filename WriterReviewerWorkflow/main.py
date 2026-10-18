from workflow import WriterReviewerWorkflow


def main() -> None:
    print("Writer-Reviewer Workflow App")
    print("Enter a high-level request and the workflow will create refined content.")

    request = input("Request: ").strip()
    if not request:
        print("No request provided. Exiting.")
        return

    workflow = WriterReviewerWorkflow()
    refined_output = workflow.execute(request)

    print("\n=== Refined Content ===\n")
    print(refined_output)


if __name__ == "__main__":
    main()
