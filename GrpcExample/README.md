## 1. GrpcInit - 기본 gRPC 프로젝트 초기화   
- gRPC 프로젝트 초기화 (dotnet new grpc)   
- 기본 gRPC 서비스 (GreeterService) 생성 및 HelloWorld 예제 구성   
- Proto 파일로 gRPC 인터페이스 정의   
- GrpcServiceBase를 상속받아 서비스 구현   
- Client로 gRPC 호출   
   
## 2. CRUD - gRPC 기반 CRUD 서비스 구현   
- gRPC 서비스에서 Create, Read, Update, Delete (CRUD) 구현.   
- user.proto에서 사용자 CRUD API 정의   
- gRPC 서비스에서 UserRepository와 연동하여 CRUD 처리   
- DTO와 gRPC 요청/응답 분리   
   
## 3. Proto_DTO_Entity - gRPC에서 DTO 및 Entity 구분   
- DTO (Data Transfer Object) → 클라이언트-서버 통신용 객체   
- Entity → DB 테이블과 직접 매핑되는 객체   
- AutoMapper로 DTO ↔ Entity 자동 변환 구성   
- gRPC에서 클라이언트에는 DTO, 서버는 Entity 사용   
   
## 4. EFCore - Entity Framework Core 통합   
- SQLite을 사용해 DB 연결   
- UserRepository → EF Core 기반 CRUD 처리   
- DbContext 구성 및 Dependency Injection 설정   
- Migration (DB Schema 자동 생성)
   
## 5. FluentValidations - gRPC 요청 유효성 검증   
- FluentValidation 패키지로 gRPC 요청 파라미터 검증   
- CreateUserRequest, UpdateUserRequest 각각에 유효성 조건 추가   
- Validator를 DI로 등록하여 자동 적용   
   
## 6. JWTAuth - JWT 기반 인증 구성   
- 클라이언트가 로그인 시 JWT 발급   
- 서버는 JWT를 검증하여 인증 처리   
- gRPC 요청 헤더에서 JWT 토큰 인증 (Interceptor)   
    
## 7. RoleAuth - 권한(Role) 기반 인가 처리   
- JWT Claims에 사용자 역할 (Role) 추가   
- 서버에서 Role 기반 권한 체크 (Admin/User)   
- 클라이언트는 JWT에 Role 포함하여 요청   
   
## 8. GlobalRpcException - gRPC 글로벌 예외 처리   
- Interceptor로 전역 예외 처리 구성   
- 예외 발생 시 RpcException으로 변환   
- 상태 코드(StatusCode.InvalidArgument, StatusCode.NotFound 등) 지정   
   
## 9. HealthCheckEndpoint - gRPC + Health Check 구성   
- REST API /health 엔드포인트 구성   
- gRPC 서버 상태 점검용 HealthCheck (Grpc.HealthCheck)   
- 클라이언트가 /health로 서버 상태 확인 가능   
   
## 10. PrometheusMetrics - gRPC 성능 지표 수집   
- Prometheus /metrics 엔드포인트 추가   
- gRPC 호출, 응답 시간, 상태 코드 등 지표 수집   
- Grafana 연동 가능   

## 11. RateLimitings - 요청 수 제한 (Rate Limiting)   
- 클라이언트 IP 또는 사용자 ID 기반 요청 제한   
- Microsoft.AspNetCore.RateLimiting 사용   
- Fixed Window, Sliding Window 정책 적용 가능   

## 12. GrpcGateway - gRPC Gateway (REST → gRPC)   
- REST API에서 내부 gRPC 서비스 직접 호출   
- JWT 인증, 예외 처리도 gRPC와 공유   
- API Gateway로 확장 가능   

## 13. GrpcClientRetryPolicy - 클라이언트 자동 재시도   
- gRPC 클라이언트에서 자동 재시도 (Retry) 정책 설정   
- Polly로 재시도 + 지수 백오프 구성   
- 일시적 오류 (StatusCode.Unavailable) 발생 시 자동 재시도   

## 14. GrpcHealthcheck - gRPC 전용 Health Check   
- gRPC HealthCheck (grpc.health.v1) 표준 서비스 구현   
- 클라이언트가 Health.Check() 호출로 상태 확인 가능   
- 서버에서 서비스별 상태 관리 가능   

## 15. GrpcStreaming - gRPC 스트리밍 (Server/Client/BiDirectional)   
- Server Streaming: 서버 → 클라이언트로 스트리밍   
- Client Streaming: 클라이언트 → 서버로 스트리밍   
- Bi-Directional Streaming: 클라이언트 ↔ 서버 양방향 스트리밍   

## 16. GrpcPollingRRLB - 채널 풀링 + 라운드로빈 로드밸런싱   
- 다중 gRPC 서버 간 라운드로빈 분산   
- GrpcChannelManager로 동적 채널 선택   
- 여러 서버 인스턴스 동시 실행 가능   

## 17. BIStreamChat_Client - 실시간 채팅 (Bi-Directional)   
- 클라이언트 간 실시간 채팅 구조 (브로드캐스트)   
- 서버가 각 클라이언트 연결 저장 → 메시지 전송 시 Broadcast   
- ConcurrentDictionary + Stream으로 클라이언트 관리   

## 18. FileUploadDownload - 파일 전송 (Upload/Download)   
- Client Streaming: 파일 업로드 (클라 → 서버)   
- Server Streaming: 파일 다운로드 (서버 → 클라)   
- 청크(Chunk) 단위 전송으로 대용량 파일 처리   

## 19. GrpcCompression - gRPC 데이터 압축 설정   
- gRPC 채널에서 gzip 압축 설정   
- 클라이언트 → 서버 요청 압축 (WriteOptions)   
- 서버 → 클라이언트 응답 압축 (CompressionLevel.Fastest)   

## 20. GrpcDeadlineTimeout - 요청 타임아웃 처리    
- 클라이언트에서 Deadline 설정 (DateTime.UtcNow.AddSeconds(3))
- 서버에서 지연 시 DeadlineExceeded 예외 발생
- 타임아웃 발생 시 자동 중단 및 예외 처리
