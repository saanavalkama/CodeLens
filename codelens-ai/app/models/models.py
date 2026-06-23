import uuid
from sqlalchemy import Column, String, Boolean, DateTime, BigInteger, Text, ForeignKey, Integer
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import DeclarativeBase
from sqlalchemy.orm import relationship
from pgvector.sqlalchemy import Vector
from datetime import datetime

class Base(DeclarativeBase):
    pass

class User(Base):
    __tablename__ = "Users"

    id = Column("Id",UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    github_id = Column("GitHubId", BigInteger)
    github_username = Column("GitHubUsername", String)
    email = Column("Email", String, nullable=True)
    github_access_token = Column("GitHubAccessToken", String)
    github_refresh_token = Column("GitHubRefreshToken", String, nullable=True)
    token_expires_at = Column("TokenExpiresAt", DateTime, nullable=True)
    encrypted_openai_key = Column("EncryptedOpenAIKey", String, nullable=True)

class Repository(Base):
    __tablename__ = "Repositories"

    id = Column("Id",UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    full_name = Column("FullName", String)
    default_branch = Column("DefaultBranch", String)
    user_id = Column("UserId", UUID(as_uuid=True))
    indexing_status = Column("IndexingStatus", Integer)
    last_progress_at = Column("LastProgressAt", DateTime, nullable=True)

class RepositoryFile(Base):
    __tablename__ = "RepositoryFiles"

    id = Column("Id",UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    repository_id = Column("RepositoryId", UUID(as_uuid=True), ForeignKey("Repositories.Id", ondelete="CASCADE"))
    path = Column("Path", String)
    sha = Column("Sha", String)
    type = Column("Type", String)
    indexing_status = Column("IndexingStatus", Integer)
    indexing_error = Column("IndexingError", String, nullable=True)
    indexed_at = Column("IndexedAt", DateTime, nullable=True)
    repository = relationship("Repository", lazy="raise")


class FileChunk(Base):
    __tablename__ = "FileChunks"

    id = Column("Id", UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    repository_file_id = Column("RepositoryFileId", UUID(as_uuid=True), ForeignKey("RepositoryFiles.Id", ondelete="CASCADE"))
    content = Column("Content", Text)
    embedding = Column("Embedding", Vector(768))
    chunk_index = Column("ChunkIndex", Integer)
    start_line = Column("StartLine", Integer)
    end_line = Column("EndLine", Integer)
    created_at = Column("CreatedAt", DateTime, default=datetime.utcnow)

class FileContent(Base):
    __tablename__ = "FileContents"

    id = Column("Id", UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    repository_file_id = Column("RepositoryFileId",UUID(as_uuid=True),ForeignKey("RepositoryFiles.Id", ondelete="CASCADE"),unique=True)
    repository_file = relationship("RepositoryFile",lazy="raise")
    content = Column("Content",Text)
    created_at = Column("CreatedAt",DateTime, default=datetime.utcnow)