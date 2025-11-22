FROM continuumio/miniconda3

WORKDIR /app

# Create the environment:
COPY environment.yml .
RUN conda env create -f environment.yml

# Make RUN commands use the new environment:
SHELL ["conda", "run", "-n", "transcript-generator", "/bin/bash", "-c"]

# Copy the application code
COPY . .

# Expose the port
EXPOSE 8000

# The code to run when container is started:
ENTRYPOINT ["conda", "run", "--no-capture-output", "-n", "transcript-generator", "uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
