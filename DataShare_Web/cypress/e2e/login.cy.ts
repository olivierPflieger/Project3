import { faker } from '@faker-js/faker'

describe('Login tests', () => {
   
  it('Register new user then login should work', () => {
    
    const url = Cypress.config('baseUrl');
    cy.log('url : ' + url)

    // fake datas    
    const email = faker.internet.email()
    const password = faker.internet.password()
    cy.log('fake user : ' + email + ', ' + password)

    // register new user
    cy.visit('/register')
    cy.get('[data-cy="register-input-email"]').type(email)
    cy.get('[data-cy="register-input-password"]').type(password)
    cy.get('[data-cy="register-input-confirmPassword"]').type(password)
    cy.get('[data-cy="register-button-submit"]').click()

    // assert
    cy.get('[data-cy="register-div-alert"]').should('contain.text', 'Inscription réussie !') 
    
    // Redirect to login page
    cy.get('[data-cy="register-link-reset"]').click()

    // login with the new user
    cy.get('[data-cy="login-input-email"]').type(email)
    cy.get('[data-cy="login-input-password"]').type(password)
    cy.get('[data-cy="login-button-submit"]').click()
    
    // assert
    cy.get('[data-cy="home-text"]').should('contain.text', 'Tu veux partager un fichier ?')
  })

  it('Login a not existing user/password should return an error', () => {
    
    const url = Cypress.config('baseUrl');
    cy.log('url : ' + url)

    // fake datas    
    const email = faker.internet.email()
    const password = faker.internet.password()
    cy.log('fake user : ' + email + ', ' + password)

    // login with the new user
    cy.visit('/login')
    cy.get('[data-cy="login-input-email"]').type(email)
    cy.get('[data-cy="login-input-password"]').type(password)
    cy.get('[data-cy="login-button-submit"]').click()

    // assert
    cy.get('[data-cy="login-div-alert"]').should('contain.text', 'Email ou mot de passe incorrect.') 
        
  })
  
  it('Login a bad user email should return an error', () => {
    
    const url = Cypress.config('baseUrl');
    cy.log('url : ' + url)

    // fake datas    
    const email = 'bad-email'
    cy.log('fake user : ' + email)

    // login with the new user
    cy.visit('/login')
    cy.get('[data-cy="login-input-email"]').type(email)
    cy.get('[data-cy="login-button-submit"]').click()

    // assert
    cy.get('[data-cy="invalid-feedback-email"]').should('contain.text', 'Veuillez saisir une adresse email valide') 
        
  })

})